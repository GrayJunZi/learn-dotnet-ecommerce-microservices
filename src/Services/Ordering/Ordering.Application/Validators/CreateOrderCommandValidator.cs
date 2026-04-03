using FluentValidation;
using Ordering.Application.Orders.CreateOrder;
using Ordering.Application.Orders.UpdateOrder;

namespace Ordering.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MaximumLength(70)
            .WithMessage("{PropertyName} must not exceed 70 characters.");

        RuleFor(x => x.TotalPrice)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .GreaterThanOrEqualTo(0)
            .WithMessage("{PropertyName} must be greater than or equal to zero.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.");

        RuleFor(x => x.CardNumber)
            .CreditCard().When(x => !string.IsNullOrEmpty(x.CardNumber))
            .WithMessage("{PropertyName} must be a valid card number.");

        RuleFor(x => x.CardExpiration)
            .Matches(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$")
            .When(x => !string.IsNullOrEmpty(x.CardExpiration))
            .WithMessage("{PropertyName} must be in MM/YY format.");

        RuleFor(x => x.CardCvv)
            .Matches(@"^\d{3,4}$")
            .When(x => !string.IsNullOrEmpty(x.CardCvv))
            .WithMessage("{PropertyName} must be 3 or  4 digits.");
    }

    public class UpdateOrderCommandValidator:AbstractValidator<UpdateOrderCommand>
    {
        public UpdateOrderCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than or equal to zero.");
            
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .MaximumLength(70)
                .WithMessage("{PropertyName} must not exceed 70 characters.");

            RuleFor(x => x.TotalPrice)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("{PropertyName} must be greater than or equal to zero.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.");

            RuleFor(x => x.CardNumber)
                .CreditCard().When(x => !string.IsNullOrEmpty(x.CardNumber))
                .WithMessage("{PropertyName} must be a valid card number.");

            RuleFor(x => x.CardExpiration)
                .Matches(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$")
                .When(x => !string.IsNullOrEmpty(x.CardExpiration))
                .WithMessage("{PropertyName} must be in MM/YY format.");

            RuleFor(x => x.CardCvv)
                .Matches(@"^\d{3,4}$")
                .When(x => !string.IsNullOrEmpty(x.CardCvv))
                .WithMessage("{PropertyName} must be 3 or  4 digits.");
        }
    }
}