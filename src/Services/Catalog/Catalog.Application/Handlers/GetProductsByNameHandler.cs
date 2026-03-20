using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetProductsByNameHandler(IProductRepository productRepository) : IRequestHandler<GetProductsByNameQuery,IList<ProductResponse>>
{
    public async Task<IList<ProductResponse>> Handle(GetProductsByNameQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetProductsByNameAsync(request.Name);
        return products.ToResponse();
    }
}