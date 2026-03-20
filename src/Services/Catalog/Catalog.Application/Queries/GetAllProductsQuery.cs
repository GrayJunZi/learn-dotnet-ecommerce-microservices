using Catalog.Application.Responses;
using Catalog.Core.Specification;
using MediatR;

namespace Catalog.Application.Queries;

public record GetAllProductsQuery(CatalogSpecParams CatalogSpecParams) : IRequest<Pagination<ProductResponse>>
{
    
}