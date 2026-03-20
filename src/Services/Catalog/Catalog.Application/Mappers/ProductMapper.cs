using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Specification;

namespace Catalog.Application.Mappers;

public static class ProductMapper
{
    public static ProductResponse ToResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Summary = product.Summary,
            Description = product.Description,
            ImageFile = product.ImageFile,
            Price = product.Price,
            Brand = product.Brand.ToResponse(),
            Type = product.Type.ToResponse(),
            CreatedDate = product.CreatedDate,
        };
    }

    public static IList<ProductResponse> ToResponse(this IEnumerable<Product> products)
    {
        return products.Select(x => x.ToResponse()).ToList();
    }

    public static Pagination<ProductResponse> ToResponse(this Pagination<Product> pagination)
        => new Pagination<ProductResponse>(
            pagination.PageIndex,
            pagination.PageSize,
            pagination.Count,
            pagination.Data.Select(x => x.ToResponse()).ToList());

    public static Product ToEntity(this CreateProductCommand command, ProductBrand brand, ProductType type)
        => new Product
        {
            Name = command.Name,
            Summary = command.Summary,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price,
            Brand = brand,
            Type = type,
            CreatedDate = DateTimeOffset.UtcNow,
        };

    public static Product ToEntity(this UpdateProductCommand command, Product product, ProductBrand brand,
        ProductType type)
        => new Product
        {
            Id = product.Id,
            Name = command.Name,
            Summary = command.Summary,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price,
            Brand = brand,
            Type = type,
            CreatedDate = product.CreatedDate,
        };

    public static ProductDto ToDto(this ProductResponse product)
    {
        if (product is null)
            return null;

        return new ProductDto
        (
            Id: product.Id,
            Name: product.Name,
            Summary: product.Summary,
            Description: product.Description,
            ImageFile: product.ImageFile,
            Brand: new BrandDto(product.Brand.Id, product.Brand.Name),
            Type: new TypeDto(product.Type.Id, product.Type.Name),
            Price: product.Price.Value,
            CreatedDate: product.CreatedDate
        );
    }

    public static UpdateProductCommand ToCommand(this UpdateProductDto product, string id)
        => new UpdateProductCommand
        {
            Id = id,
            Name = product.Name,
            Summary = product.Summary,
            Description = product.Description,
            ImageFile = product.ImageFile,
            Price = product.Price,
            BrandId = product.BrandId,
            TypeId = product.TypeId,
        };
}