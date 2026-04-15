using EventBus.Messages.Events;
using Ordering.Application.DTOs;
using Ordering.Application.Orders.CreateOrder;
using Ordering.Application.Orders.UpdateOrder;
using Ordering.Core.Entities;

namespace Ordering.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order)
        => new OrderDto(
            order.Id,
            order.UserName,
            order.TotalPrice ?? 0,
            order.Name,
            order.EmailAddress,
            order.AddressLine,
            order.Country,
            order.State,
            order.ZipCode,
            order.CardName,
            order.CardNumber,
            order.CardExpiration,
            order.Cvv,
            order.PaymentMethod ?? 0);

    public static Order ToEntity(this CreateOrderCommand command)
        => new Order
        {
            UserName = command.UserName,
            TotalPrice = command.TotalPrice ?? 0,
            Name = command.Name,
            EmailAddress = command.EmailAddress,
            AddressLine = command.AddressLine,
            Country = command.Country,
            State = command.State,
            ZipCode = command.ZipCode,
            CardName = command.CardName,
            CardNumber = command.CardNumber,
            CardExpiration = command.CardExpiration,
            Cvv = command.Cvv,
            PaymentMethod = command.PaymentMethod
        };

    public static void ApplyUpdate(this Order order, UpdateOrderCommand command)
    {
        order.UserName = command.UserName;
        order.TotalPrice = command.TotalPrice;
        order.Name = command.Name;
        order.EmailAddress = command.EmailAddress;
        order.AddressLine = command.AddressLine;
        order.Country = command.Country;
        order.State = command.State;
        order.ZipCode = command.ZipCode;
        order.CardName = command.CardName;
        order.CardNumber = command.CardNumber;
        order.CardExpiration = command.CardExpiration;
        order.Cvv = command.Cvv;
        order.PaymentMethod = command.PaymentMethod;
    }

    public static CreateOrderCommand ToCommand(this CreateOrderDto createOrderDto)
        => new CreateOrderCommand
        {
            UserName = createOrderDto.UserName,
            TotalPrice = createOrderDto.TotalPrice,
            Name = createOrderDto.Name,
            EmailAddress = createOrderDto.EmailAddress,
            AddressLine = createOrderDto.AddressLine,
            Country = createOrderDto.Country,
            State = createOrderDto.State,
            ZipCode = createOrderDto.ZipCode,
            CardName = createOrderDto.CardName,
            CardNumber = createOrderDto.CardNumber,
            CardExpiration = createOrderDto.CardExpiration,
            Cvv = createOrderDto.Cvv,
            PaymentMethod = createOrderDto.PaymentMethod
        };

    public static UpdateOrderCommand ToCommand(this OrderDto orderDto)
        => new UpdateOrderCommand
        {
            Id = orderDto.Id,
            UserName = orderDto.UserName,
            TotalPrice = orderDto.TotalPrice,
            Name = orderDto.Name,
            EmailAddress = orderDto.EmailAddress,
            AddressLine = orderDto.AddressLine,
            Country = orderDto.Country,
            State = orderDto.State,
            ZipCode = orderDto.ZipCode,
            CardName = orderDto.CardName,
            CardNumber = orderDto.CardNumber,
            CardExpiration = orderDto.CardExpiration,
            Cvv = orderDto.Cvv,
            PaymentMethod = orderDto.PaymentMethod
        };

    public static CreateOrderCommand ToCheckoutOrderCommand(this BasketCheckoutEvent basketCheckoutEvent)
        => new CreateOrderCommand
        {
            UserName = basketCheckoutEvent.UserName,
            TotalPrice = basketCheckoutEvent.TotalPrice,
            Name = basketCheckoutEvent.Name,
            EmailAddress = basketCheckoutEvent.EmailAddress,
            AddressLine = basketCheckoutEvent.AddressLine,
            Country = basketCheckoutEvent.Country,
            State = basketCheckoutEvent.State,
            ZipCode = basketCheckoutEvent.ZipCode,
            CardName = basketCheckoutEvent.CardName,
            CardNumber = basketCheckoutEvent.CardNumber,
            CardExpiration = basketCheckoutEvent.CardExpiration,
            Cvv = basketCheckoutEvent.Cvv,
            PaymentMethod = basketCheckoutEvent.PaymentMethod
        };
}