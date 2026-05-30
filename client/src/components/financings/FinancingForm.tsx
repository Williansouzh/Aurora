import { useState } from 'react';
import { amortizationSystems, financingTypes } from '../../constants/financeOptions';
import { enumValue } from '../../utils/enumHelpers';
import { todayInput } from '../../utils/formatters';
import { formatCurrency } from '../../lib/utils';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';

const defaultForm = {
  name: '',
  type: 'Home',
  amortizationSystem: 'SAC',
  institution: '',
  assetValue: 500000,
  downPayment: 100000,
  annualInterestRate: 10.5,
  monthlyInsurance: 85,
  monthlyFees: 25,
  cetAnnualRate: '',
  termMonths: 360,
  firstDueDate: todayInput(),
  linkedAccountId: '',
  notes: '',
  propertyAddress: '',
  propertyRegistration: '',
  vehicleBrand: '',
  vehicleModel: '',
  vehicleYear: '',
  vehiclePlate: '',
};

function toPayload(form) {
  return {
    userId: '',
    name: form.name,
    institution: form.institution,
    type: enumValue(financingTypes, form.type),
    amortizationSystem: enumValue(amortizationSystems, form.amortizationSystem),
    assetValue: Number(form.assetValue),
    downPayment: Number(form.downPayment),
    annualInterestRate: Number(form.annualInterestRate),
    monthlyInsurance: Number(form.monthlyInsurance),
    monthlyFees: Number(form.monthlyFees),
    cetAnnualRate: form.cetAnnualRate === '' ? null : Number(form.cetAnnualRate),
    termMonths: Number(form.termMonths),
    firstDueDate: form.firstDueDate,
    linkedAccountId: form.linkedAccountId || null,
    notes: form.notes || null,
    propertyAddress: form.propertyAddress || null,
    propertyRegistration: form.propertyRegistration || null,
    vehicleBrand: form.vehicleBrand || null,
    vehicleModel: form.vehicleModel || null,
    vehicleYear: form.vehicleYear ? Number(form.vehicleYear) : null,
    vehiclePlate: form.vehiclePlate || null,
  };
}

function fromFinancing(f) {
  return {
    name: f.name,
    type: financingTypes[f.type]?.[0] ?? 'Home',
    amortizationSystem: amortizationSystems[f.amortizationSystem]?.[0] ?? 'SAC',
    institution: f.institution || '',
    assetValue: f.assetValue,
    downPayment: f.downPayment,
    annualInterestRate: f.annualInterestRate,
    monthlyInsurance: f.monthlyInsurance,
    monthlyFees: f.monthlyFees,
    cetAnnualRate: f.cetAnnualRate ?? '',
    termMonths: f.termMonths,
    firstDueDate: f.firstDueDate?.slice(0, 10) ?? todayInput(),
    linkedAccountId: f.linkedAccountId ?? '',
    notes: f.notes ?? '',
    propertyAddress: f.propertyAddress ?? '',
    propertyRegistration: f.propertyRegistration ?? '',
    vehicleBrand: f.vehicleBrand ?? '',
    vehicleModel: f.vehicleModel ?? '',
    vehicleYear: f.vehicleYear ?? '',
    vehiclePlate: f.vehiclePlate ?? '',
  };
}

const selectClass =
  'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring';

export function FinancingForm({ api, initialValues, onSubmit, onCancel }) {
  const [form, setForm] = useState(() =>
    initialValues ? fromFinancing(initialValues) : { ...defaultForm },
  );
  const [simulation, setSimulation] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const isVehicle = form.type === 'Vehicle';
  const isHome = form.type === 'Home';
  const set = (key) => (e) => setForm((prev) => ({ ...prev, [key]: e.target.value }));

  async function simulate() {
    setError('');
    try {
      const result = await api.post('/api/financings/simulate', toPayload(form));
      setSimulation(result);
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await onSubmit(toPayload(form));
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="fin-name">Nome</Label>
          <Input id="fin-name" value={form.name} onChange={set('name')} required />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-institution">Instituição</Label>
          <Input id="fin-institution" value={form.institution} onChange={set('institution')} placeholder="Banco, financeira..." />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-type">Tipo</Label>
          <select
            id="fin-type"
            value={form.type}
            onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))}
            className={selectClass}
          >
            {financingTypes.map(([val, label]) => (
              <option key={val} value={val}>{label}</option>
            ))}
          </select>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-amort">Sistema de amortização</Label>
          <select
            id="fin-amort"
            value={form.amortizationSystem}
            onChange={(e) => setForm((p) => ({ ...p, amortizationSystem: e.target.value }))}
            className={selectClass}
          >
            {amortizationSystems.map(([val, label]) => (
              <option key={val} value={val}>{label}</option>
            ))}
          </select>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-asset">Valor do bem</Label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
            <Input id="fin-asset" type="number" step="0.01" value={form.assetValue} onChange={set('assetValue')} required className="pl-8" />
          </div>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-down">Entrada</Label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
            <Input id="fin-down" type="number" step="0.01" value={form.downPayment} onChange={set('downPayment')} required className="pl-8" />
          </div>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-rate">Juros ao ano (%)</Label>
          <Input id="fin-rate" type="number" step="0.01" value={form.annualInterestRate} onChange={set('annualInterestRate')} required />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-cet">
            CET ao ano (%) <span className="text-muted-foreground text-xs">opcional</span>
          </Label>
          <Input id="fin-cet" type="number" step="0.01" value={form.cetAnnualRate} onChange={set('cetAnnualRate')} placeholder="—" />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-insurance">Seguro mensal</Label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
            <Input id="fin-insurance" type="number" step="0.01" value={form.monthlyInsurance} onChange={set('monthlyInsurance')} className="pl-8" />
          </div>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-fees">Taxas mensais</Label>
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">R$</span>
            <Input id="fin-fees" type="number" step="0.01" value={form.monthlyFees} onChange={set('monthlyFees')} className="pl-8" />
          </div>
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-term">Prazo (meses)</Label>
          <Input id="fin-term" type="number" value={form.termMonths} onChange={set('termMonths')} required />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="fin-due">1º vencimento</Label>
          <Input id="fin-due" type="date" value={form.firstDueDate} onChange={set('firstDueDate')} required />
        </div>

        {isHome && (
          <>
            <div className="space-y-1.5">
              <Label htmlFor="fin-address">Endereço do imóvel</Label>
              <Input id="fin-address" value={form.propertyAddress} onChange={set('propertyAddress')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="fin-reg">Matrícula</Label>
              <Input id="fin-reg" value={form.propertyRegistration} onChange={set('propertyRegistration')} />
            </div>
          </>
        )}

        {isVehicle && (
          <>
            <div className="space-y-1.5">
              <Label htmlFor="fin-brand">Marca</Label>
              <Input id="fin-brand" value={form.vehicleBrand} onChange={set('vehicleBrand')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="fin-model">Modelo</Label>
              <Input id="fin-model" value={form.vehicleModel} onChange={set('vehicleModel')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="fin-year">Ano</Label>
              <Input id="fin-year" type="number" value={form.vehicleYear} onChange={set('vehicleYear')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="fin-plate">Placa</Label>
              <Input id="fin-plate" value={form.vehiclePlate} onChange={set('vehiclePlate')} />
            </div>
          </>
        )}

        <div className="space-y-1.5 sm:col-span-2">
          <Label htmlFor="fin-notes">Observações</Label>
          <textarea
            id="fin-notes"
            value={form.notes}
            onChange={set('notes')}
            rows={2}
            className="flex min-h-[60px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      {simulation && (
        <div className="rounded-lg border bg-muted/40 p-4 grid grid-cols-2 sm:grid-cols-4 gap-3 text-sm">
          <div>
            <p className="text-xs text-muted-foreground">Financiado</p>
            <p className="font-semibold tabular-nums">{formatCurrency(simulation.financedAmount)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">1ª parcela</p>
            <p className="font-semibold tabular-nums">{formatCurrency(simulation.firstPayment)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Última parcela</p>
            <p className="font-semibold tabular-nums">{formatCurrency(simulation.lastPayment)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Juros totais</p>
            <p className="font-semibold tabular-nums text-destructive">{formatCurrency(simulation.totalInterest)}</p>
          </div>
        </div>
      )}

      <div className="flex items-center justify-between gap-3 pt-2">
        <div className="flex gap-2">
          <Button type="button" variant="outline" onClick={onCancel}>Cancelar</Button>
          <Button type="button" variant="secondary" onClick={simulate}>Simular</Button>
        </div>
        <Button type="submit" disabled={loading}>
          {loading ? 'Salvando...' : initialValues ? 'Atualizar' : 'Salvar'}
        </Button>
      </div>
    </form>
  );
}
