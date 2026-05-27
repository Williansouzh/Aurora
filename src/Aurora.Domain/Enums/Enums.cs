namespace Aurora.Domain.Enums;
public enum AccountType { CheckingAccount, Savings, Cash, Investment, CreditCard }
public enum TransactionType { Income, Expense, Transfer }
public enum TransactionStatus { Paid, Pending, Overdue, Canceled }
public enum RecurrenceType { Monthly, Weekly, Yearly, Custom }
public enum CategoryType { Income, Expense }
public enum FinancingType { Home, Vehicle, Other }
public enum AmortizationSystem { SAC, Price }
public enum FinancingStatus { Active, PaidOff, Paused }
public enum FinancingInstallmentStatus { Planned, Paid, Overdue }
public enum CreditCardInvoiceStatus { Open, Closed, Paid }
public enum TransferStatus { Completed, Pending, Cancelled }
