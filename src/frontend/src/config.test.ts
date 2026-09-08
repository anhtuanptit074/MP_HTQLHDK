import { describe, expect, it } from 'vitest';

import { normalizeEnvironmentName, safeLogFields } from './config';

describe('frontend bootstrap configuration', () => {
  it('defaults missing environment to Local', () => {
    expect(normalizeEnvironmentName(undefined)).toBe('Local');
  });

  it('does not echo an untrusted environment value', () => {
    expect(normalizeEnvironmentName('production<script>')).toBe('Unknown');
  });

  it('does not allow sensitive fields in the log allowlist', () => {
    expect(safeLogFields).not.toContain('attack');
    expect(safeLogFields).not.toContain('evidence');
    expect(safeLogFields).not.toContain('rawObjectKey');
    expect(safeLogFields).not.toContain('token');
  });
});
