import { transactionStatuses, transactionTypes } from '../../constants/financeOptions';
import { periodPresets } from '../../hooks/useTransactionFilters';
import { Button } from '../ui/button';
import { Card, CardContent } from '../ui/card';
import { Label } from '../ui/label';

const toggleId = (list, id) => list.includes(id) ? list.filter((x) => x !== id) : [...list, id];

export function TransactionFilters({ filters, setFilters, resetFilters, expanded, accounts = [], categories = [] }) {
  if (!expanded) return null;

  return (
    <Card>
      <CardContent className="p-4">
        <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-4">
          <div className="space-y-1.5">
            <Label>Período</Label>
            <select
              value={filters.periodPreset}
              onChange={(e) => setFilters({ periodPreset: e.target.value })}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring"
            >
              {periodPresets.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
            </select>
          </div>

          {filters.periodPreset === 'custom' && (
            <>
              <div className="space-y-1.5">
                <Label>De</Label>
                <input type="date" value={filters.customFrom} onChange={(e) => setFilters({ customFrom: e.target.value })}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring" />
              </div>
              <div className="space-y-1.5">
                <Label>Até</Label>
                <input type="date" value={filters.customTo} onChange={(e) => setFilters({ customTo: e.target.value })}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring" />
              </div>
            </>
          )}

          <div className="space-y-1.5">
            <Label>Tipo</Label>
            <select value={filters.type} onChange={(e) => setFilters({ type: e.target.value })}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring">
              <option value="">Todos</option>
              {transactionTypes.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
            </select>
          </div>

          <div className="space-y-1.5">
            <Label>Status</Label>
            <select value={filters.status} onChange={(e) => setFilters({ status: e.target.value })}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring">
              <option value="">Todos</option>
              {transactionStatuses.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
            </select>
          </div>

          <div className="space-y-1.5">
            <Label>Valor de (R$)</Label>
            <input type="number" step="0.01" min="0" value={filters.minAmount} onChange={(e) => setFilters({ minAmount: e.target.value })}
              placeholder="0,00"
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring" />
          </div>

          <div className="space-y-1.5">
            <Label>Valor até (R$)</Label>
            <input type="number" step="0.01" min="0" value={filters.maxAmount} onChange={(e) => setFilters({ maxAmount: e.target.value })}
              placeholder="0,00"
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring" />
          </div>
        </div>

        {/* Checkboxes for accounts and categories */}
        <div className="mt-4 grid grid-cols-1 gap-4 md:grid-cols-2">
          {accounts.length > 0 && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground mb-2">Contas</p>
              <div className="flex flex-wrap gap-1.5">
                {accounts.map((a) => (
                  <button
                    key={a.id}
                    type="button"
                    onClick={() => setFilters({ accountIds: toggleId(filters.accountIds, a.id) })}
                    className={`rounded-full border px-2.5 py-1 text-xs font-medium transition-colors border-0 min-h-0 cursor-pointer ${
                      filters.accountIds.includes(a.id)
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
                    }`}
                  >
                    {a.name}
                  </button>
                ))}
              </div>
            </div>
          )}

          {categories.length > 0 && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground mb-2">Categorias</p>
              <div className="flex flex-wrap gap-1.5">
                {categories.map((c) => (
                  <button
                    key={c.id}
                    type="button"
                    onClick={() => setFilters({ categoryIds: toggleId(filters.categoryIds, c.id) })}
                    className={`rounded-full border px-2.5 py-1 text-xs font-medium transition-colors border-0 min-h-0 cursor-pointer ${
                      filters.categoryIds.includes(c.id)
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
                    }`}
                  >
                    {c.name}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className="mt-4 flex justify-end">
          <Button variant="outline" size="sm" onClick={resetFilters}>Limpar filtros</Button>
        </div>
      </CardContent>
    </Card>
  );
}
