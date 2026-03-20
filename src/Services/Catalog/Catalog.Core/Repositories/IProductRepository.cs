using Catalog.Core.Entities;
using Catalog.Core.Specification;

namespace Catalog.Core.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Pagination<Product>> GetProductsAsync(CatalogSpecParams catalogSpecParams);
    Task<IEnumerable<Product>> GetProductsByNameAsync(string name);
    Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand);
    Task<Product> GetProductAsync(string productId);
    Task<Product> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string productId);
    Task<ProductBrand> GetBrandsByIdAsync(string brandId);
    Task<ProductType> GetTypesByIdAsync(string typeId);
}