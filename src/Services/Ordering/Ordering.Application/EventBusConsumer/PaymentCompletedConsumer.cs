using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.Repositories;

namespace Ordering.Application.EventBusConsumer;

public class PaymentCompletedConsumer(
    IOrderRepository orderRepository,
    ILogger<PaymentCompletedConsumer> logger) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order not found for Id: {OrderId} and CorrelationId: {CorrelationId}",
                context.Message.OrderId, context.CorrelationId);
            return;
        }

        order.Status = Core.Enums.OrderStatus.Paid;
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Order Id {OrderId} marked as Paid", context.Message.OrderId);
    }
}