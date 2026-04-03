using Microsoft.Extensions.Logging;
using Ordering.Application.Abstractions;
using Ordering.Application.Exceptions;
using Ordering.Application.Mappers;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Orders.UpdateOrder;

public class UpdateOrderHandler(
    IOrderRepository orderRepository,
    ILogger<UpdateOrderHandler> logger) : ICommandHandler<UpdateOrderCommand>
{
    public async Task Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id);
        if (order is null)
            throw new OrderNotFoundException(nameof(Order), command.Id);

        order.ApplyUpdate(command);
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Order {Id} is successfully updated", command.Id);
    }
}