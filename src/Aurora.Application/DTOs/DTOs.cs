using Aurora.Domain.Enums;
namespace Aurora.Application.DTOs;
public record AuthRequest(string Name,string Email,string Password);
public record LoginRequest(string Email,string Password);
public record AuthResponse(string Token,string UserId,string Name,string Email);
public record AccountRequest(string Name, AccountType Type, decimal InitialBalance, string Color);
public record CategoryRequest(string Name, CategoryType Type, string Color, string Icon);
public record TransactionRequest(string AccountId,string CategoryId,string Description,decimal Amount,TransactionType Type,TransactionStatus Status,DateTime Date,DateTime? DueDate,string? Notes);
