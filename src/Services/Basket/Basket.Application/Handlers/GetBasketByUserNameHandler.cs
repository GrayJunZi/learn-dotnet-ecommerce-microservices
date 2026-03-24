using Basket.Application.Mappers;
using Basket.Application.Queries;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class GetBasketByUserNameHandler(IBasketRepository basketRepository)
    : IRequestHandler<GetBasketByUserNameQuery, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(GetBasketByUserNameQuery request,
        CancellationToken cancellationToken)
    {
        var shoppingCart = await basketRepository.GetBasket(request.UserName);
        if (shoppingCart is null)
        {
            return new ShoppingCartResponse
            {
                Items = new List<ShoppingCartItemResponse>()
            };
        }

        return shoppingCart.ToResponse();
    }
}