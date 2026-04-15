using Basket.Application.Commands;
using Basket.Application.GrpcServices;
using Basket.Application.Mappers;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class CreateShoppingCartHandler(
    IBasketRepository basketRepository,
    DiscountGrpcService discountGrpcService) : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var coupon = await discountGrpcService.GetDiscount(item.ProductName);
            item.Price -= coupon.Amount;
        }

        var shoppingCart = request.ToEntity();
        var updatedCart = await basketRepository.UpdateBasket(shoppingCart);
        return updatedCart.ToResponse();
    }
}