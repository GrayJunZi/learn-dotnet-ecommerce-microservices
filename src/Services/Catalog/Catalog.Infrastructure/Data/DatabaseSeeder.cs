using System.Text.Json;
using Catalog.Core.Entities;
using Catalog.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Data;

public class DatabaseSeeder
{
    public static async Task SeedAsync(IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        var brands = database.GetCollection<ProductBrand>(settings.BrandCollectionName);
        var types = database.GetCollection<ProductType>(settings.TypeCollectionName);
        var products = database.GetCollection<Product>(settings.ProductCollectionName);

        var seedBasePath = Path.Combine("Data", "SeedData");

        // Seed Brands
        var brandList = await brands.Find(_ => true).ToListAsync();
        if (brandList.Count == 0)
        {
            var brandData = await File.ReadAllTextAsync(Path.Combine(seedBasePath, "brands.json"));
            brandList = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);
            await brands.InsertManyAsync(brandList);
        }

        // Seed Types
        var typeList = await types.Find(_ => true).ToListAsync();
        if (typeList.Count == 0)
        {
            var typeData = await File.ReadAllTextAsync(Path.Combine(seedBasePath, "types.json"));
            typeList = JsonSerializer.Deserialize<List<ProductType>>(typeData);
            await types.InsertManyAsync(typeList);
        }

        // Seed Products
        var productList = await products.Find(_ => true).ToListAsync();
        if (productList.Count == 0)
        {
            var productData = await File.ReadAllTextAsync(Path.Combine(seedBasePath, "products.json"));
            productList = JsonSerializer.Deserialize<List<Product>>(productData);
            await products.InsertManyAsync(productList);
        }
    }
}