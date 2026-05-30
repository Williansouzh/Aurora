import { useCallback, useEffect, useMemo, useState } from 'react';
import { AlertsDropdown, clientDaysUntilDue } from './AlertsDropdown';
import { useToast } from '../../hooks/useToast';

export function AlertsBell({ api }) {
  const toast = useToast();
  const [open, setOpen] = useState(false);
  const [alerts, setAlerts] = useState([]);
  const [dayKey, setDayKey] = useState(todayKey);

  const loadAlerts = useCallback(async () => {
    try {
      setAlerts(await api.get('/api/dashboard/upcoming-dues?days=7&status=pending'));
    } catch {
      setAlerts([]);
    }
  }, [api]);

  useEffect(() => {
    loadAlerts();
  }, [loadAlerts, dayKey]);

  useEffect(() => {
    const interval = window.setInterval(() => setDayKey(todayKey()), 60 * 60 * 1000);
    return () => window.clearInterval(interval);
  }, []);

  const urgentAlerts = useMemo(
    () => alerts
      .map((alert) => ({ ...alert, daysUntilDue: clientDaysUntilDue(alert.dueDate) }))
      .filter((alert) => alert.daysUntilDue <= 3)
      .sort((a, b) => a.daysUntilDue - b.daysUntilDue),
    [alerts],
  );

  const markPaid = async (id) => {
    try {
      await api.patch(`/api/transactions/${id}/mark-as-paid`);
      await loadAlerts();
      toast.success('Transação atualizada');
    } catch {
      toast.error('Erro ao salvar. Tente novamente.');
    }
  };

  return (
    <div className="alerts-bell">
      <button type="button" className="alerts-bell-button" aria-label="Alertas" onClick={() => setOpen((current) => !current)}>
        <span aria-hidden="true">🔔</span>
        {urgentAlerts.length > 0 && <strong>{urgentAlerts.length}</strong>}
      </button>
      {open && <AlertsDropdown alerts={urgentAlerts} onMarkPaid={markPaid} onClose={() => setOpen(false)} />}
    </div>
  );
}

function todayKey() {
  return new Date().toISOString().slice(0, 10);
}
