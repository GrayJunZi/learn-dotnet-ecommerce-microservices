using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using Catalog.Core.Specification;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Handlers;

public class GetAllProductsHandler(
    IProductRepository productRepository,
    ILogger<GetAllProductsHandler> logger)
    : IRequestHandler<GetAllProductsQuery, Pagination<ProductResponse>>
{
    public async Task<Pagination<ProductResponse>> Handle(GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var productList = await productRepository.GetProductsAsync(request.CatalogSpecParams);
        logger.LogInformation("Fetched {ProductCount} products", productList.Count);
        return productList.ToResponse();
    }
}