const USER_KEY = 'aurora.user';

export function getStoredUser() {
  const raw = readStorage(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    clearAuth();
    return null;
  }
}

export function storeUser(user) {
  writeStorage(USER_KEY, JSON.stringify(user));
}

export function clearAuth() {
  removeStorage(USER_KEY);
}

function readStorage(key) {
  try { return window.localStorage.getItem(key); } catch { return null; }
}

function writeStorage(key, value) {
  try { window.localStorage.setItem(key, value); } catch {}
}

function removeStorage(key) {
  try { window.localStorage.removeItem(key); } catch {}
}
