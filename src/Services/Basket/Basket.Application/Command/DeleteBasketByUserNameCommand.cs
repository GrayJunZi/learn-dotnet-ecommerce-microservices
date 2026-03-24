using MediatR;

namespace Basket.Application.Command;

public record DeleteBasketByUserNameCommand(string UserName) : IRequest<Unit>;