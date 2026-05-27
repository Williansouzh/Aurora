using System.Globalization;
using System.Text;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Transactions.ExportCsv;

public record ExportTransactionsCsvQuery(
    string UserId,
    int? Month,
    int? Year,
    TransactionType? Type,
    TransactionStatus? Status,
    string? CategoryId,
    string? AccountId,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<TransactionsCsvFile>;

public record TransactionsCsvFile(string FileName, string Content);

public class ExportTransactionsCsvHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICategoryRepository catRepo) : IRequestHandler<ExportTransactionsCsvQuery, TransactionsCsvFile>
{
    public async Task<TransactionsCsvFile> Handle(ExportTransactionsCsvQuery query, CancellationToken ct)
    {
        var filter = new TransactionFilter(
            query.UserId, query.Month, query.Year, query.Type, query.Status,
            query.CategoryId, query.AccountId, null, query.DateFrom, query.DateTo,
            null, null, null, null);

        var (items, _) = await txRepo.GetPagedAsync(filter, 1, int.MaxValue);
        var accounts = (await accRepo.GetByUserAsync(query.UserId)).ToDictionary(x => x.Id, x => x.Name);
        var categories = (await catRepo.GetByUserAsync(query.UserId)).ToDictionary(x => x.Id, x => x.Name);

        var sb = new StringBuilder();
        sb.AppendLine("Data;Descricao;Tipo;Categoria;Conta;Valor;Status;Vencimento;Notas");

        foreach (var tx in items.OrderByDescending(x => x.Date))
        {
            var row = string.Join(";", new[]
            {
                tx.Date.ToString("yyyy-MM-dd"),
                CsvEscape(tx.Description),
                tx.Type.ToString(),
                CsvEscape(categories.GetValueOrDefault(tx.CategoryId, string.Empty)),
                CsvEscape(accounts.GetValueOrDefault(tx.AccountId, string.Empty)),
                tx.Amount.ToString(CultureInfo.InvariantCulture),
                tx.Status.ToString(),
                tx.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                CsvEscape(tx.Notes ?? string.Empty)
            });
            sb.AppendLine(row);
        }

        var monthLabel = (query.DateFrom?.Month ?? query.Month ?? DateTime.UtcNow.Month).ToString("00");
        var yearLabel = (query.DateFrom?.Year ?? query.Year ?? DateTime.UtcNow.Year).ToString();

        return new TransactionsCsvFile($"aurora-transacoes-{monthLabel}-{yearLabel}.csv", sb.ToString());
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var needsQuotes = value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
