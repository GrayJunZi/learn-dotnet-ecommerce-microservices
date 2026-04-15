using Discount.Application.Commands;
using Discount.Application.Mappers;
using Discount.Application.Queries;
using Discount.Grpc.Protos;
using Grpc.Core;
using MediatR;

namespace Discount.API.Services;

public class DiscountService(IMediator mediator) : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var query = new GetDiscountQuery(request.ProductName);
        var result = await mediator.Send(query);
        return result.ToModel();
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var command = request.Coupon.ToCreateCommand();
        var result = await mediator.Send(command);
        return result.ToModel();
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var command = request.Coupon.ToUpdateCommand();
        var result = await mediator.Send(command);
        return result.ToModel();
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        var command = new DeleteDiscountCommand(request.ProductName);
        var result = await mediator.Send(command);
        return new DeleteDiscountResponse
        {
            Success = result
        };
    }
}