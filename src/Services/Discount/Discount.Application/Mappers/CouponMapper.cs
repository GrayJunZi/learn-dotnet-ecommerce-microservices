using Discount.Application.Commands;
using Discount.Application.DTOs;
using Discount.Core.Entities;
using Discount.Grpc.Protos;

namespace Discount.Application.Mappers;

public static class CouponMapper
{
    public static CouponDto ToDto(this Coupon coupon)
        => new CouponDto(coupon.Id, coupon.ProductName, coupon.Description, coupon.Amount);

    public static Coupon ToEntity(this CreateDiscountCommand command)
        => new Coupon
        {
            ProductName = command.ProductName,
            Description = command.Description,
            Amount = command.Amount
        };

    public static Coupon ToEntity(this UpdateDiscountCommand command)
        => new Coupon
        {
            Id = command.Id,
            ProductName = command.ProductName,
            Description = command.Description,
            Amount = command.Amount
        };

    public static CouponModel ToModel(this CouponDto coupon)
        => new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount,
        };

    public static CreateDiscountCommand ToCreateCommand(this CouponModel coupon)
        => new CreateDiscountCommand(coupon.ProductName, coupon.Description, coupon.Amount);

    public static UpdateDiscountCommand ToUpdateCommand(this CouponModel coupon)
        => new UpdateDiscountCommand(coupon.Id, coupon.ProductName, coupon.Description, coupon.Amount);
}