using Aurora.Application.Features;
using Aurora.Application.Features.Auth;
using FluentValidation;

namespace Aurora.Application.Validators;
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand> {
 public RegisterUserCommandValidator(){ RuleFor(x=>x.Name).NotEmpty().MaximumLength(120); RuleFor(x=>x.Email).NotEmpty().EmailAddress(); RuleFor(x=>x.Password).NotEmpty().MinimumLength(6); }
}
public class LoginCommandValidator : AbstractValidator<LoginCommand> { public LoginCommandValidator(){ RuleFor(x=>x.Email).NotEmpty().EmailAddress(); RuleFor(x=>x.Password).NotEmpty(); }}
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand> { public CreateAccountCommandValidator(){ RuleFor(x=>x.Name).NotEmpty().MaximumLength(80); RuleFor(x=>x.InitialBalance).GreaterThanOrEqualTo(0); }}
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand> { public CreateTransactionCommandValidator(){ RuleFor(x=>x.AccountId).NotEmpty(); RuleFor(x=>x.CategoryId).NotEmpty(); RuleFor(x=>x.Description).NotEmpty().MaximumLength(200); RuleFor(x=>x.Amount).GreaterThan(0); }}
public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand> { public UpdateTransactionCommandValidator(){ RuleFor(x=>x.Id).NotEmpty(); RuleFor(x=>x.AccountId).NotEmpty(); RuleFor(x=>x.CategoryId).NotEmpty(); RuleFor(x=>x.Amount).GreaterThan(0); }}
