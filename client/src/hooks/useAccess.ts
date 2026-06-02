import { useCallback, useEffect, useState } from 'react';

export function useAccess(api, user) {
  const [access, setAccess] = useState(null);
  const [loading, setLoading] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [error, setError] = useState('');

  const reload = useCallback(async () => {
    if (!user) {
      setAccess(null);
      setLoaded(true);
      setError('');
      return null;
    }

    setLoading(true);
    setError('');
    try {
      const snapshot = await api.get('/api/access/me');
      setAccess(snapshot);
      setLoaded(true);
      return snapshot;
    } catch (err) {
      setAccess(null);
      setLoaded(true);
      setError(err.message);
      return null;
    } finally {
      setLoading(false);
    }
  }, [api, user]);

  useEffect(() => {
    setLoaded(false);
    reload();
  }, [reload]);

  const canAccess = useCallback((moduleKey) => {
    if (!access) return true;
    return access.modules?.some((module) => module.key === moduleKey && module.isAllowed);
  }, [access]);

  return { access, loading: Boolean(user) && (loading || !loaded), error, reload, canAccess };
}
