using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly IMongoCollection<ProductBrand> _brands;

    public BrandRepository(IMongoClient client, IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var database = client.GetDatabase(settings.DatabaseName);
        _brands = database.GetCollection<ProductBrand>(settings.BrandCollectionName);
    }

    public async Task<IEnumerable<ProductBrand>> GetAllBrandsAsync()
    {
        return await _brands.Find(_ => true).ToListAsync();
    }

    public async Task<ProductBrand> GetBrandAsync(string brandId)
    {
        return await _brands.Find(x => x.Id == brandId).FirstOrDefaultAsync();
    }
}