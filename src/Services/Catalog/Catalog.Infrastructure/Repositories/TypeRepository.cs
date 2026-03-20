using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class TypeRepository : ITypeRepository
{
    private readonly IMongoCollection<ProductType> _types;

    public TypeRepository(IMongoClient client,IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var database = client.GetDatabase(settings.DatabaseName);
        _types = database.GetCollection<ProductType>(settings.TypeCollectionName);
    }

    public async Task<IEnumerable<ProductType>> GetAllTypesAsync()
    {
        return await _types.Find(_ => true).ToListAsync();
    }

    public async Task<ProductType> GetByIdAsync(string typeId)
    {
        return await _types.Find(x => x.Id == typeId).FirstOrDefaultAsync();
    }
}