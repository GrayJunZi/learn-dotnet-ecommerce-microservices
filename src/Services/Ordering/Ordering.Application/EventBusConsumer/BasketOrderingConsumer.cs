using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Abstractions;
using Ordering.Application.Mappers;
using Ordering.Application.Orders.CreateOrder;

namespace Ordering.Application.EventBusConsumer;

public class BasketOrderingConsumer(
    ICommandHandler<CreateOrderCommand, int> createOrderCommandHandler,
    ILogger<BasketOrderingConsumer> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        using var scope = logger
            .BeginScope("Consuming BasketCheckoutEvent for {CorrelationId}", context.Message.CorrelationId);

        var command = context.Message.ToCheckoutOrderCommand();
        var orderId = await createOrderCommandHandler.Handle(command, context.CancellationToken);
        logger.LogInformation("BasketCheckoutEvent Completed Successfully!! OrderId: {OrderId}", orderId);
    }
}