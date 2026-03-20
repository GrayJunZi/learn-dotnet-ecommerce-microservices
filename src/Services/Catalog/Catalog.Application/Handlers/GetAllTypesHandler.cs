using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllTypesHandler(ITypeRepository typeRepository) : IRequestHandler<GetAllTypesQuery, IList<TypeResponse>>
{
    public async Task<IList<TypeResponse>> Handle(GetAllTypesQuery request, CancellationToken cancellationToken)
    {
        var typeList = await typeRepository.GetAllTypesAsync();
        return typeList.ToResponse();
    }
}