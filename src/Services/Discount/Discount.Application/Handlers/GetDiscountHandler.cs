using Discount.Application.DTOs;
using Discount.Application.Extensions;
using Discount.Application.Mappers;
using Discount.Application.Queries;
using Discount.Core.Repositories;
using Grpc.Core;
using MediatR;

namespace Discount.Application.Handlers;

public class GetDiscountHandler(IDiscountRepository discountRepository) : IRequestHandler<GetDiscountQuery, CouponDto>
{
    public async Task<CouponDto> Handle(GetDiscountQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            var validationErrors = new Dictionary<string, string>()
            {
                { "ProductName", "Product name must be empty." },
            };
            throw GrpcErrorHelper.CreateValidationException(validationErrors);
        }

        var coupon = await discountRepository.GetDiscount(request.ProductName);
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.Internal,
                $"Could not get discount for product: {request.ProductName}"));
        return coupon.ToDto();
    }
}