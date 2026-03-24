using Basket.Application.Command;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class DeleteBasketByUserNameHandler(IBasketRepository basketRepository) : IRequestHandler<DeleteBasketByUserNameCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBasketByUserNameCommand request, CancellationToken cancellationToken)
    {
        await  basketRepository.DeleteBasket(request.UserName);
        return Unit.Value;
    }
}