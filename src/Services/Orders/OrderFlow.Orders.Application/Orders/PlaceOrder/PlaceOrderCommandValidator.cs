using FluentValidation;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.Lines)
            .NotEmpty()
            .WithErrorCode("validation.required")
            .Must(lines => lines.Count <= PlaceOrderCommand.MaxLines)
            .WithErrorCode("validation.max_items")
            .Must(lines => lines.Select(line => line.ProductId).Distinct().Count() == lines.Count)
            .WithErrorCode("validation.duplicate");
        RuleForEach(command => command.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(item => item.ProductId)
                    .NotEmpty()
                    .WithErrorCode("validation.required");
                line.RuleFor(item => item.Quantity)
                    .InclusiveBetween(1, PlaceOrderCommand.MaxQuantity)
                    .WithErrorCode("validation.range");
            });

        RuleFor(command => command.ShippingAddress)
            .NotNull()
            .WithErrorCode("validation.required")
            .ChildRules(address =>
            {
                address.RuleFor(a => a.FullName).Length(2, 100).WithErrorCode("validation.length");
                address.RuleFor(a => a.Line1).Length(3, 200).WithErrorCode("validation.length");
                address.RuleFor(a => a.Line2).MaximumLength(200).WithErrorCode("validation.length");
                address.RuleFor(a => a.City).Length(2, 100).WithErrorCode("validation.length");
                address.RuleFor(a => a.PostalCode).Length(3, 20).WithErrorCode("validation.length");
                address.RuleFor(a => a.Country).Length(2, 56).WithErrorCode("validation.length");
                address
                    .RuleFor(a => a.Phone)
                    .Matches(@"^\+?[0-9 ()-]{7,20}$")
                    .WithErrorCode("validation.phone");
            });

        RuleFor(command => command.PaymentMethod)
            .Must(method => method is PaymentMethods.Wallet or PaymentMethods.Card)
            .WithErrorCode("validation.payment_method");
        RuleFor(command => command.CardPaymentId)
            .NotEmpty()
            .When(command => command.PaymentMethod == PaymentMethods.Card)
            .WithErrorCode("validation.required");
        RuleFor(command => command.Scenario)
            .Must(scenario => scenario is null or OrderScenarios.LostParcel)
            .WithErrorCode("validation.scenario");
    }
}
