using Discount.Application.Commands;
using Discount.Application.DTOs;
using Discount.Application.Extensions;
using Discount.Application.Mappers;
using Discount.Core.Repositories;
using Grpc.Core;
using MediatR;

namespace Discount.Application.Handlers;

public class UpdateDiscountHandler(IDiscountRepository discountRepository) : IRequestHandler<UpdateDiscountCommand, CouponDto>
{
    public async  Task<CouponDto> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        var validationErrors = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(request.ProductName))
            validationErrors["ProductName"]="Product name must not be empty.";
        if (string.IsNullOrWhiteSpace(request.Description))
            validationErrors["Description"] = "Product Description must not be empty.";
        if (request.Amount <= 0)
            validationErrors["Amount"] = "Amount must be greater than zero.";
        if (validationErrors.Any())
            throw GrpcErrorHelper.CreateValidationException(validationErrors);

        var coupon = request.ToEntity();
        
        var isUpdated = await discountRepository.UpdateDiscount(coupon);
        if (!isUpdated)
            throw new RpcException(new Status(StatusCode.Internal,
                $"Could not create discount for product: {request.ProductName}"));
        
        return coupon.ToDto();
    }
}