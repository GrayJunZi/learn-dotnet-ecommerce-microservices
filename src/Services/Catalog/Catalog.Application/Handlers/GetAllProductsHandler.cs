using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using Catalog.Core.Specification;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllProductsHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsQuery, Pagination<ProductResponse>>
{
    public async Task<Pagination<ProductResponse>> Handle(GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var productList = await productRepository.GetProductsAsync(request.CatalogSpecParams);
        return productList.ToResponse();
    }
}