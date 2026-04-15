using Basket.Application.Commands;
using Basket.Application.Mappers;
using Basket.Application.Queries;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Basket.Application.Handlers;

public class BasketCheckoutHandler(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger<BasketCheckoutHandler> logger) : IRequestHandler<BasketCheckoutCommand, Unit>
{
    public async Task<Unit> Handle(BasketCheckoutCommand request, CancellationToken cancellationToken)
    {
        var basketDto = request.BasketCheckoutDto;
        var basketResponse = await mediator.Send(new GetBasketByUserNameQuery(basketDto.UserName), cancellationToken);
        if (basketResponse is null || !basketResponse.Items.Any())
        {
            throw new InvalidOperationException("Basket not found or empty");
        }

        var basket = basketResponse.ToEntity();

        var @event = basketDto.ToBasketCheckoutEvent(basket);
        logger.LogInformation("Publishing BasketCheckoutEvent for {User}", basketDto.UserName);
        await publishEndpoint.Publish(@event, cancellationToken);
        await mediator.Send(new DeleteBasketByUserNameCommand(basketDto.UserName), cancellationToken);
        return Unit.Value;
    }
}