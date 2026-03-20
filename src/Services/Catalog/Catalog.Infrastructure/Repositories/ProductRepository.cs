using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specification;
using Catalog.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductBrand> _brands;
    private readonly IMongoCollection<ProductType> _types;
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(IMongoClient client, IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var database = client.GetDatabase(settings.DatabaseName);
        _brands = database.GetCollection<ProductBrand>(settings.BrandCollectionName);
        _types = database.GetCollection<ProductType>(settings.TypeCollectionName);
        _products = database.GetCollection<Product>(settings.ProductCollectionName);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _products.Find(_ => true).ToListAsync();
    }

    public async Task<Pagination<Product>> GetProductsAsync(CatalogSpecParams catalogSpecParams)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(catalogSpecParams.Search))
        {
            filter &= builder.Where(x => x.Name.ToLower().Contains(catalogSpecParams.Search.ToLower()));
        }

        if (!string.IsNullOrEmpty(catalogSpecParams.BrandId))
        {
            filter &= builder.Eq(x => x.Brand.Id, catalogSpecParams.BrandId);
        }

        if (!string.IsNullOrEmpty(catalogSpecParams.TypeId))
        {
            filter &= builder.Eq(x => x.Type.Id, catalogSpecParams.TypeId);
        }

        var totalCount = await _products.CountDocumentsAsync(filter);
        var data = await ApplyDataFilter(catalogSpecParams, filter);
        return new Pagination<Product>(catalogSpecParams.PageIndex, catalogSpecParams.PageSize, totalCount, data);
    }

    public async Task<IEnumerable<Product>> GetProductsByNameAsync(string name)
    {
        var filter = Builders<Product>.Filter.Regex(x => x.Name, new BsonRegularExpression($".*{name}.*", "i"));
        return await _products.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand)
    {
        return await _products.Find(x => x.Brand.Name.ToLower().Contains(brand.ToLower())).ToListAsync();
    }

    public async Task<Product> GetProductAsync(string productId)
    {
        return await _products.Find(x => x.Id == productId).FirstOrDefaultAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await _products.InsertOneAsync(product);
        return product;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        var updatedProduct = await _products.ReplaceOneAsync(x => x.Id == product.Id, product);
        return updatedProduct.IsAcknowledged && updatedProduct.ModifiedCount > 0;
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        var deletedProduct = await _products.DeleteOneAsync(x => x.Id == productId);
        return deletedProduct.IsAcknowledged && deletedProduct.DeletedCount > 0;
    }

    public async Task<ProductBrand> GetBrandsByIdAsync(string brandId)
    {
        return await _brands.Find(x => x.Id == brandId).FirstOrDefaultAsync();
    }

    public async Task<ProductType> GetTypesByIdAsync(string typeId)
    {
        return await _types.Find(x => x.Id == typeId).FirstOrDefaultAsync();
    }


    private async Task<IReadOnlyCollection<Product>> ApplyDataFilter(CatalogSpecParams catalogSpecParams,
        FilterDefinition<Product> filter)
    {
        var sortDefinition = Builders<Product>.Sort.Ascending("Name");
        if (!string.IsNullOrEmpty(catalogSpecParams.Sort))
        {
            sortDefinition = catalogSpecParams.Sort switch
            {
                "priceAsc" => Builders<Product>.Sort.Ascending(x => x.Price),
                "priceDesc" => Builders<Product>.Sort.Descending(x => x.Price),
                _ => Builders<Product>.Sort.Ascending(x => x.Name)
            };
        }

        return await _products.Find(filter)
            .Sort(sortDefinition)
            .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
            .Limit(catalogSpecParams.PageSize)
            .ToListAsync();
    }
}