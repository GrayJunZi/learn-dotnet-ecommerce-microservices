namespace Ordering.Application.DTOs;

public record CreateOrderDto(
    string UserName,
    decimal TotalPrice,
    string Name,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string CardName,
    string CardNumber,
    string CardExpiration,
    string Cvv,
    int PaymentMethod);

public record OrderDto(
    int Id,
    string UserName,
    decimal TotalPrice,
    string Name,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string CardName,
    string CardNumber,
    string CardExpiration,
    string Cvv,
    int PaymentMethod);