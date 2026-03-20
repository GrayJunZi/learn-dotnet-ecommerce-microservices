using Catalog.Application.Commands;
using Catalog.Application.Mappers;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductAsync(request.Id);
        if (product is null)
            throw new KeyNotFoundException($"Product with Id {request.Id} not found");

        var brand = await productRepository.GetBrandsByIdAsync(request.BrandId);
        if (brand is null)
            throw new ApplicationException("Brand not found");

        var type = await productRepository.GetTypesByIdAsync(request.TypeId);
        if (type is null)
            throw new ApplicationException("Type not found");

        var updatedProduct = request.ToEntity(product, brand, type);
        return await productRepository.UpdateProductAsync(updatedProduct);
    }
}