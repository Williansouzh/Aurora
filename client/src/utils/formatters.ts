export const todayInput = () => new Date().toISOString().slice(0, 10);

export const money = (value) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(value || 0));

export const dateText = (value) => (value ? new Date(value).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '-');
