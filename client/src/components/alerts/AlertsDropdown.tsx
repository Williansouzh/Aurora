import { Link } from 'react-router-dom';
import { money } from '../../utils/formatters';

export function AlertsDropdown({ alerts = [], onMarkPaid, onClose }) {
  return (
    <div className="alerts-dropdown">
      <div className="alerts-head">
        <strong>Alertas de vencimento</strong>
        <button type="button" onClick={onClose} aria-label="Fechar alertas">x</button>
      </div>

      <div className="alerts-list">
        {alerts.length === 0 && <p className="empty">Nenhuma pendência urgente.</p>}
        {alerts.map((alert) => {
          const days = clientDaysUntilDue(alert.dueDate);
          return (
            <div className="alert-row" key={alert.id}>
              <div>
                <strong>{alert.description}</strong>
                <small>{money(alert.amount)}</small>
              </div>
              <span className={`due-badge ${urgencyTone(days)}`}>{urgencyLabel(days)}</span>
              <button type="button" onClick={() => onMarkPaid(alert.id)}>Pagar</button>
            </div>
          );
        })}
      </div>

      <Link className="alerts-footer" to="/transactions" onClick={onClose}>
        Ver todas as pendências
      </Link>
    </div>
  );
}

export function clientDaysUntilDue(value) {
  if (!value) return 999;
  const today = new Date();
  const due = new Date(value);
  const todayOnly = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
  const dueOnly = Date.UTC(due.getUTCFullYear(), due.getUTCMonth(), due.getUTCDate());
  return Math.round((dueOnly - todayOnly) / 86400000);
}

export function urgencyLabel(days) {
  if (days < 0) return 'Atrasada';
  if (days === 0) return 'Hoje';
  if (days === 1) return 'Amanhã';
  return `Em ${days} dias`;
}

export function urgencyTone(days) {
  if (days < 0) return 'danger';
  if (days === 0) return 'today';
  if (days === 1) return 'warning';
  return 'neutral';
}
