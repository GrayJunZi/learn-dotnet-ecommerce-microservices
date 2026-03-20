using Catalog.Application.Commands;
using Catalog.Application.Mappers;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var brand = await productRepository.GetBrandsByIdAsync(request.BrandId);
        if (brand is null)
            throw new ApplicationException($"Brand {request.BrandId} not found");

        var type = await productRepository.GetTypesByIdAsync(request.TypeId);
        if (type is null)
            throw new ApplicationException($"Type {request.TypeId} not found");

        var createdProduct = request.ToEntity(brand, type);
        var product = await productRepository.CreateProductAsync(createdProduct);
        return product.ToResponse();
    }
}