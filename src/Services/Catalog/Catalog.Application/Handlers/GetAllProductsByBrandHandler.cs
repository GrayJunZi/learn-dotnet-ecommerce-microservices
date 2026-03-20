using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllProductsByBrandHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsByBrandQuery, IList<ProductResponse>>
{
    public async Task<IList<ProductResponse>> Handle(GetAllProductsByBrandQuery request,
        CancellationToken cancellationToken)
    {
       var productList= await productRepository.GetProductsByBrandAsync(request.BrandName);
       return productList.ToResponse();
    }
}