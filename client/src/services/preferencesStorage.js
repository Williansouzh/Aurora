const KEY = 'aurora.preferences';

const defaults = {
  dashboardMonth: 'current',
  currency: 'BRL',
  pageSize: 20,
};

export function getPreferences() {
  try {
    const raw = window.localStorage.getItem(KEY);
    if (!raw) return { ...defaults };
    return { ...defaults, ...JSON.parse(raw) };
  } catch {
    return { ...defaults };
  }
}

export function savePreferences(preferences) {
  try {
    window.localStorage.setItem(KEY, JSON.stringify(preferences));
  } catch {
    // Preferences are best-effort; ignore storage failures.
  }
}
