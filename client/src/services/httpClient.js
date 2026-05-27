import { API_BASE_URL } from '../config/api';
import { clearMemoryToken, getMemoryToken, setMemoryToken } from './authMemory';

let _onUnauthorized = null;
export function setUnauthorizedCallback(fn) { _onUnauthorized = fn; }

let _refreshPromise = null;

async function silentRefresh() {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    });
    if (!response.ok) return false;
    const body = await response.json();
    setMemoryToken(body.data?.accessToken);
    return true;
  } catch {
    return false;
  }
}

async function parseResponse(response) {
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.message || 'Nao foi possivel concluir a operacao.');
  }
  const body = await response.json();
  return body.data;
}

async function rawRequest(path, options = {}, isRetry = false) {
  const token = getMemoryToken();
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers || {}),
    },
  });

  if (response.status === 401 && !isRetry) {
    if (!_refreshPromise) {
      _refreshPromise = silentRefresh().finally(() => { _refreshPromise = null; });
    }
    const refreshed = await _refreshPromise;
    if (refreshed) return rawRequest(path, options, true);
    clearMemoryToken();
    _onUnauthorized?.();
    throw new Error('Sessão expirada. Faça login novamente.');
  }

  return parseResponse(response);
}

export function createHttpClient() {
  const download = async (path) => {
    const token = getMemoryToken();
    const response = await fetch(`${API_BASE_URL}${path}`, {
      credentials: 'include',
      headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    });
    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      throw new Error(body.message || 'Nao foi possivel concluir a operacao.');
    }
    const disposition = response.headers.get('content-disposition') || '';
    const match = /filename\*?=(?:UTF-8'')?\"?([^\";]+)\"?/i.exec(disposition);
    const filename = match ? decodeURIComponent(match[1]) : 'download';
    const blob = await response.blob();
    return { filename, blob };
  };

  return {
    get: (path) => rawRequest(path),
    post: (path, body) => rawRequest(path, { method: 'POST', body: JSON.stringify(body) }),
    put: (path, body) => rawRequest(path, { method: 'PUT', body: JSON.stringify(body) }),
    patch: (path, body) => rawRequest(path, { method: 'PATCH', ...(body ? { body: JSON.stringify(body) } : {}) }),
    delete: (path, body) => rawRequest(path, { method: 'DELETE', ...(body ? { body: JSON.stringify(body) } : {}) }),
    download,
  };
}
