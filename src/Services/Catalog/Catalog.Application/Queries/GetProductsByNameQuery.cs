using Catalog.Application.Responses;
using MediatR;

namespace Catalog.Application.Queries;

public record GetProductsByNameQuery(string Name) : IRequest<IList<ProductResponse>>
{
    
}