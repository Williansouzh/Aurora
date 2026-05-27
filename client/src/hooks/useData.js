import { useEffect, useState } from 'react';

export function useData(loader, deps = []) {
  const [state, setState] = useState({ data: null, loading: true, error: '' });

  const reload = () => {
    setState((current) => ({ ...current, loading: true, error: '' }));

    return loader()
      .then((data) => setState({ data, loading: false, error: '' }))
      .catch((err) => setState({ data: null, loading: false, error: err.message }));
  };

  useEffect(() => {
    reload();
  }, deps);

  return { ...state, reload };
}
