using Ordering.Application.Abstractions;

namespace Ordering.Application.Orders.UpdateOrder;

public class UpdateOrderCommand : ICommand
{
    public int Id { get; init; }
    public string? UserName { get; init; }
    public decimal? TotalPrice { get; init; }
    public string? Name { get; init; }
    public string? EmailAddress { get; init; }
    public string? AddressLine { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? CardName { get; init; }
    public string? CardNumber { get; init; }
    public string? CardExpiration { get; init; }
    public string? Cvv { get; init; }
    public int? PaymentMethod { get; init; }
}