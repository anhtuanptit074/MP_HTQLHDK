import { describe, expect, it } from 'vitest';
import { actions } from './forms';
import type { Inputs } from './forms';

function inputs(grants: Inputs['context']['grants'], officer = 7): Inputs {
  return {
    context: {
      userId: 7,
      displayName: 'Test',
      grants,
      isAdmin: grants.some((g) => g.role === 'ADMIN'),
      assets: { items: [{ id: 11, securityOfficerId: officer }] },
    },
    users: [],
    profiles: [],
    scopes: [],
    assets: [],
  };
}
describe('business action visibility', () => {
  it('does not give an administrator severity or raw-report actions', () => {
    const admin = inputs([{ id: 1, role: 'ADMIN', assetId: null }]);
    expect(actions('findings', { id: 1, assetId: 11, status: 'New' }, admin)).toEqual([]);
    expect(
      actions(
        'runs',
        { id: 1, assetId: 11, operatorId: 8, scanStatus: 'Completed', reportStatus: 'Processed' },
        admin,
      ),
    ).toEqual([]);
  });
  it('requires a scoped security grant and the responsible officer', () => {
    const row = { id: 1, assetId: 11, status: 'New' };
    expect(actions('findings', row, inputs([{ id: 1, role: 'SECURITY', assetId: null }]))).toEqual(
      [],
    );
    expect(actions('findings', row, inputs([{ id: 1, role: 'SECURITY', assetId: 11 }], 9))).toEqual(
      [],
    );
    expect(
      actions('findings', row, inputs([{ id: 1, role: 'SECURITY', assetId: 11 }])).some((a) =>
        a.spec.path.endsWith('/assess'),
      ),
    ).toBe(true);
  });
  it('requires actual ticket assignment for progress updates', () => {
    const context = inputs([{ id: 1, role: 'REMEDIATOR', assetId: 11 }]);
    const row = {
      id: 3,
      assetId: 11,
      assigneeId: 9,
      status: 'Assigned',
      finding: { assetId: 11, status: 'Assigned' },
    };
    expect(actions('tickets', row, context)).toEqual([]);
    expect(
      actions('tickets', { ...row, assigneeId: 7 }, context).some((a) =>
        a.spec.path.endsWith('/progress'),
      ),
    ).toBe(true);
  });
  it('shows verification decisions only for processed verification runs', () => {
    const context = inputs([{ id: 1, role: 'SECURITY', assetId: 11 }]);
    const row = {
      id: 1,
      assetId: 11,
      planKind: 'Verification',
      scanStatus: 'Completed',
      reportStatus: 'Rejected',
    };
    expect(
      actions('runs', row, context).some((a) => a.spec.path.endsWith('/verification-decision')),
    ).toBe(false);
    expect(
      actions('runs', { ...row, reportStatus: 'Processed' }, context).some((a) =>
        a.spec.path.endsWith('/verification-decision'),
      ),
    ).toBe(true);
  });
});
