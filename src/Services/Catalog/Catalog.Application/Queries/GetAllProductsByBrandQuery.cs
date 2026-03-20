using Catalog.Application.Responses;
using MediatR;

namespace Catalog.Application.Queries;

public record GetAllProductsByBrandQuery(string BrandName) : IRequest<IList<ProductResponse>>
{
    
}