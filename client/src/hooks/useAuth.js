import { useEffect, useRef, useState } from 'react';
import { clearAuth, getStoredUser, storeUser } from '../services/authStorage';
import { clearMemoryToken, setMemoryToken } from '../services/authMemory';
import { createHttpClient, setUnauthorizedCallback } from '../services/httpClient';
import { API_BASE_URL } from '../config/api';

const api = createHttpClient();

export function useAuth() {
  const [user, setUser] = useState(getStoredUser);
  const [ready, setReady] = useState(false);
  const signOutRef = useRef(null);

  const signOut = async () => {
    try { await api.post('/api/auth/logout', {}); } catch {}
    clearMemoryToken();
    clearAuth();
    setUser(null);
  };

  signOutRef.current = signOut;

  useEffect(() => {
    setUnauthorizedCallback(() => {
      clearMemoryToken();
      clearAuth();
      setUser(null);
    });
  }, []);

  useEffect(() => {
    fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    })
      .then(async (response) => {
        if (!response.ok) { clearAuth(); setUser(null); return; }
        const body = await response.json();
        setMemoryToken(body.data?.accessToken);
        return api.get('/api/auth/me').then((me) => {
          const nextUser = { ...(getStoredUser() || {}), ...me };
          storeUser(nextUser);
          setUser(nextUser);
        }).catch(() => {});
      })
      .catch(() => { clearAuth(); setUser(null); })
      .finally(() => setReady(true));
  }, []);

  const signIn = (auth) => {
    setMemoryToken(auth.accessToken);
    const nextUser = { userId: auth.userId, name: auth.name, email: auth.email };
    storeUser(nextUser);
    setUser(nextUser);
  };

  const updateUser = (next) => {
    const merged = { ...(user || {}), ...next };
    storeUser(merged);
    setUser(merged);
  };

  return { api, ready, user, signIn, signOut, updateUser };
}
