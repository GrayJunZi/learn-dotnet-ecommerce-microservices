using Basket.Application.Command;
using Basket.Application.Mappers;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class CreateShoppingCartHandler(IBasketRepository basketRepository) : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async  Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
    {
        var shoppingCart = request.ToEntity();
        var updatedCart = await basketRepository.UpdateBasket(shoppingCart);
        return updatedCart.ToResponse();
    }
}