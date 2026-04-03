using Ordering.Application.Abstractions;
using Ordering.Application.Mappers;
using Ordering.Core.Repositories;

namespace Ordering.Application.Orders.CreateOrder;

public class CreateOrderHandler(IOrderRepository orderRepository) : ICommandHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = command.ToEntity();
        var result = await orderRepository.AddAsync(order);
        return result.Id;
    }
}