import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from './button';

export function Pagination({ page, pageSize, totalCount, totalPages, onPageChange, itemLabel = 'transações' }) {
  if (totalCount === 0) return null;

  const safeTotalPages = Math.max(totalPages || 1, 1);
  const currentPage = Math.min(Math.max(page || 1, 1), safeTotalPages);
  const start = (currentPage - 1) * pageSize;
  const shownTo = Math.min(start + pageSize, totalCount);
  const shownCount = Math.max(shownTo - start, 0);
  const isFirstPage = currentPage <= 1;
  const isLastPage = currentPage >= safeTotalPages;

  return (
    <nav className="flex items-center justify-between py-3 border-t border-border mt-2" aria-label="Paginação">
      <span className="text-xs text-muted-foreground">
        {shownCount} de {totalCount} {itemLabel}
      </span>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="sm" disabled={isFirstPage} onClick={() => onPageChange(currentPage - 1)} className="h-7 px-2 text-xs">
          <ChevronLeft className="h-3.5 w-3.5" />
          Anterior
        </Button>
        <span className="text-xs text-muted-foreground">
          {currentPage} / {safeTotalPages}
        </span>
        <Button variant="outline" size="sm" disabled={isLastPage} onClick={() => onPageChange(currentPage + 1)} className="h-7 px-2 text-xs">
          Próxima
          <ChevronRight className="h-3.5 w-3.5" />
        </Button>
      </div>
    </nav>
  );
}
