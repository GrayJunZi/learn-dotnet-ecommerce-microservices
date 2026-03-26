using Discount.Application.Commands;
using Discount.Application.DTOs;
using Discount.Application.Extensions;
using Discount.Application.Mappers;
using Discount.Core.Repositories;
using Grpc.Core;
using MediatR;

namespace Discount.Application.Handlers;

public class CreateDiscountHandler(IDiscountRepository discountRepository) : IRequestHandler<CreateDiscountCommand, CouponDto>
{
    public async  Task<CouponDto> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
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
        
        var isCreated = await discountRepository.CreateDiscount(coupon);
        if (!isCreated)
            throw new RpcException(new Status(StatusCode.Internal,
                $"Could not create discount for product: {request.ProductName}"));
        
        return coupon.ToDto();
    }
}