import { Building2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button } from '../ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { Progress } from '../ui/progress';
import { formatCurrency } from '../../lib/utils';

export function FinancingSummaryCard({ summary }) {
  if (!summary || summary.activeCount === 0) return null;

  const progress = Number(summary.overallProgress || 0);

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between pb-3">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-50">
            <Building2 className="h-4 w-4 text-indigo-600" />
          </div>
          <CardTitle className="text-sm font-semibold">Financiamentos ativos</CardTitle>
        </div>
        <Button asChild variant="ghost" size="sm" className="h-7 text-xs text-muted-foreground">
          <Link to="/financings">Ver todos</Link>
        </Button>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <div>
            <p className="text-xs text-muted-foreground">Contratos</p>
            <p className="text-xl font-bold">{summary.activeCount}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Saldo devedor</p>
            <p className="text-xl font-bold tabular-nums">{formatCurrency(summary.totalRemainingBalance)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Parcela mensal</p>
            <p className="text-xl font-bold tabular-nums text-red-700">{formatCurrency(summary.totalMonthlyPayment)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground mb-2">Progresso geral</p>
            <div className="flex items-center gap-2">
              <Progress value={progress} indicatorClassName="bg-indigo-500" className="flex-1 bg-indigo-100" />
              <span className="text-xs font-semibold text-indigo-700 tabular-nums shrink-0">{progress.toFixed(1)}%</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
