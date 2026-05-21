using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.Repositories;

namespace Ordering.Application.EventBusConsumer;

public class PaymentFailedConsumer(
    IOrderRepository orderRepository,
    ILogger<PaymentFailedConsumer> logger) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order not found for Id: {OrderId} and CorrelationId: {CorrelationId}",
                context.Message.OrderId, context.CorrelationId);
            return;
        }

        order.Status = Core.Enums.OrderStatus.Failed;
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Payment failed for Order Id {OrderId}, Reason: {Reason}", context.Message.OrderId,
            context.Message.Reason);
    }
}