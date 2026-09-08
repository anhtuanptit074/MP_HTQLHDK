const safeEnvironmentNames = new Set(['Local', 'Test', 'Staging', 'Production']);

export function normalizeEnvironmentName(value: string | undefined): string {
  if (!value) {
    return 'Local';
  }

  const normalized = value.trim();
  return safeEnvironmentNames.has(normalized) ? normalized : 'Unknown';
}

export const safeLogFields = Object.freeze([
  'correlationId',
  'action',
  'outcome',
  'durationMs',
  'recordCount',
]);
