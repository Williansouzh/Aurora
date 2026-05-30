import { useData } from './useData';

export function useFinancings(api) {
  const list = useData(() => api.get('/api/financings'), []);
  const summary = useData(() => api.get('/api/dashboard/financing-summary'), []);

  async function create(data) {
    const result = await api.post('/api/financings', data);
    list.reload();
    summary.reload();
    return result;
  }

  async function update(id, data) {
    const result = await api.put(`/api/financings/${id}`, data);
    list.reload();
    summary.reload();
    return result;
  }

  async function remove(id) {
    await api.delete(`/api/financings/${id}`);
    list.reload();
    summary.reload();
  }

  async function markPaid(financingId, number, body = {}) {
    const result = await api.patch(
      `/api/financings/${financingId}/installments/${number}/mark-as-paid`,
      body,
    );
    list.reload();
    return result;
  }

  return { list, summary, create, update, remove, markPaid };
}
