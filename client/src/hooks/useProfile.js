import { useEffect, useState } from 'react';

export function useProfile({ api, initialUser } = {}) {
  const [profile, setProfile] = useState(initialUser || null);
  const [loading, setLoading] = useState(!initialUser);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api.get('/api/auth/me')
      .then((me) => {
        if (cancelled) return;
        setProfile((current) => ({ ...(current || {}), ...me }));
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err.message);
      })
      .finally(() => {
        if (cancelled) return;
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [api]);

  const updateProfile = async (payload) => {
    const updated = await api.put('/api/auth/profile', payload);
    setProfile((current) => ({ ...(current || {}), ...updated }));
    return updated;
  };

  const updatePassword = (payload) => api.put('/api/auth/password', payload);

  return { profile, loading, error, updateProfile, updatePassword };
}
