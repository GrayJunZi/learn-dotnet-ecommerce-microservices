using Microsoft.Extensions.Logging;
using Ordering.Application.Abstractions;
using Ordering.Application.Exceptions;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Orders.DeleteOrder;

public class DeleteOrderHandler(
    IOrderRepository orderRepository,
    ILogger<DeleteOrderHandler> logger) : ICommandHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id);
        if (order is null)
            throw new OrderNotFoundException(nameof(Order), command.Id);
        await orderRepository.DeleteAsync(order);
        logger.LogInformation("Order with id {Id} was successfully deleted", command.Id);
    }
}