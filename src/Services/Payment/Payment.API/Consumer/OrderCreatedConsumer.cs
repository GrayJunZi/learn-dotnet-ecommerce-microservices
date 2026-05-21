using EventBus.Messages.Events;
using MassTransit;

namespace Payment.API.Consumer;

public class OrderCreatedConsumer(
    IPublishEndpoint publishEndpoint,
    ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing Payment or Order Id: {OrderId}", message.Id);

        await Task.Delay(1000);

        if (message.TotalPrice > 0)
        {
            var completedEvent = new PaymentCompletedEvent
            {
                OrderId = message.Id,
                CorrelationId = context.CorrelationId.Value,
            };

            await publishEndpoint.Publish(completedEvent);
            logger.LogInformation(
                "Payment successfully completed for Order Id: {OrderId} and Correlation Id: {Correlation}",
                message.Id, context.CorrelationId);
        }
        else
        {
            var failedEvent = new PaymentFailedEvent
            {
                OrderId = message.Id,
                CorrelationId = context.CorrelationId.Value,
                Reason = "Total price was zero or negative.",
            };

            await publishEndpoint.Publish(failedEvent);
            logger.LogWarning(
                "Payment failed for Order Id: {OrderId} and Correlation Id: {Correlation}",
                message.Id, context.CorrelationId);
        }
    }
}