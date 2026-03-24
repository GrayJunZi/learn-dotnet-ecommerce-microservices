# learn-dotnet-ecommerce-microservices

基于 .NET 10 与 Angular 21 构建的生产级微服务电商平台，运用整洁架构、CQRS、Saga 模式及事件驱动架构，实现高可用、可扩展的分布式系统。

## 一、介绍

这是一个深度、以生产为先、架构驱动的实践项目，专为不仅想构建微服务，更希望正确构建微服务的开发者设计。

### 技术栈

| 分类 | 技术 |
|------|------|
| 核心框架 | .NET 10、Angular 21 |
| 架构模式 | 整洁架构（六边形架构、端口和适配器模式） |
| 设计模式 | CQRS、Saga 模式、策略模式、仓库模式、规范模式 |
| 通信 | RabbitMQ（异步消息）、gRPC（高性能通信） |
| 数据存储 | SQL Server、PostgreSQL、MongoDB、Redis |
| 容器与编排 | Docker、Kubernetes |
| 网关与服务网格 | Ocelot、NGINX、Istio |

### 项目特色

- **CQRS 与 MediatR** - 清晰的读写分离，优化性能和扩展性
- **SAGA 模式** - 复杂业务流程协调，分布式事务处理
- **Outbox 模式** - 确保可靠的消息传递
- **事件驱动架构** - 实现松耦合系统
- **完整的微服务生态** - 产品目录、购物车、订单、折扣与支付、身份与安全

---

## 二、Catalog 微服务

Catalog 微服务负责产品目录管理，采用整洁架构设计，使用 MongoDB 作为数据存储。

### 1. 项目结构

```
src/Services/Catalog/
├── Catalog.API/              # API 层 - 控制器、程序入口
├── Catalog.Application/      # 应用层 - CQRS 命令与查询处理
├── Catalog.Core/             # 核心层 - 实体、仓储接口、规格
└── Catalog.Infrastructure/   # 基础设施层 - 仓储实现、配置
```

### 2. 项目依赖关系

```
Catalog.API → Catalog.Application, Catalog.Infrastructure
Catalog.Infrastructure → Catalog.Application
Catalog.Application → Catalog.Core
```

### 3. NuGet 包依赖

| 项目 | 包名 | 版本 |
|------|------|------|
| Catalog.API | Microsoft.AspNetCore.OpenApi | 10.0.0 |
| Catalog.Application | MediatR | 14.1.0 |
| Catalog.Core | MongoDB.Driver | 3.7.0 |
| Catalog.Infrastructure | Swashbuckle.AspNetCore | 10.1.5 |

---

### 4. 核心层 (Catalog.Core)

#### 4.1 实体类

```csharp
// Entities/BaseEntity.cs
public abstract class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}

// Entities/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public string ImageFile { get; set; }
    public ProductBrand? Brand { get; set; }
    public ProductType? Type { get; set; }
    public decimal? Price { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
```

#### 4.2 规格类

```csharp
// Specification/CatalogSpecParams.cs - 查询参数
public class CatalogSpecParams
{
    private const int MaxPageSize = 70;
    public int PageIndex { get; set; } = 1;
    private int _pageSize = 10;
    public int PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; 
    }
    public string? BrandId { get; set; }
    public string? TypeId { get; set; }
    public string? Sort { get; set; }
    public string? Search { get; set; }
}
```

#### 4.3 仓储接口

```csharp
// Repositories/IProductRepository.cs
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Pagination<Product>> GetProductsAsync(CatalogSpecParams catalogSpecParams);
    Task<Product> GetProductAsync(string productId);
    Task<Product> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string productId);
    Task<ProductBrand> GetBrandsByIdAsync(string brandId);
    Task<ProductType> GetTypesByIdAsync(string typeId);
}
```

---

### 5. 基础设施层 (Catalog.Infrastructure)

#### 5.1 数据库配置

```csharp
// Settings/DatabaseSettings.cs
public class DatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string BrandCollectionName { get; set; }
    public string TypeCollectionName { get; set; }
    public string ProductCollectionName { get; set; }
}
```

#### 5.2 仓储实现

```csharp
// Repositories/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<ProductBrand> _brands;
    private readonly IMongoCollection<ProductType> _types;

    public ProductRepository(IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _products = database.GetCollection<Product>(settings.ProductCollectionName);
        _brands = database.GetCollection<ProductBrand>(settings.BrandCollectionName);
        _types = database.GetCollection<ProductType>(settings.TypeCollectionName);
    }

    public async Task<Pagination<Product>> GetProductsAsync(CatalogSpecParams param)
    {
        // 构建过滤条件：搜索、品牌、类型筛选
        // 应用排序和分页
        // 返回分页结果
    }
}
```

---

### 6. 应用层 (Catalog.Application)

#### 6.1 数据库种子数据

```csharp
// Data/DatabaseSeeder.cs
public class DatabaseSeeder
{
    public static async Task SeedAsync(IOptions<DatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        
        var seedBasePath = Path.Combine("Data", "SeedData");
        
        // 从 JSON 文件读取并插入品牌、类型、产品数据
        if (await brands.Find(_ => true).CountDocumentsAsync() == 0)
        {
            var brandData = await File.ReadAllTextAsync(Path.Combine(seedBasePath, "brands.json"));
            await brands.InsertManyAsync(JsonSerializer.Deserialize<List<ProductBrand>>(brandData));
        }
    }
}
```

**种子数据文件**：`Data/SeedData/brands.json`、`types.json`、`products.json`

#### 6.2 DTOs 与 Responses

```csharp
// DTOs/ProductDto.cs
public record ProductDto(string Id, string Name, string Summary, string Description,
    string ImageFile, BrandDto Brand, TypeDto Type, decimal Price, DateTimeOffset CreatedDate);

public record class CreateProductDto
{
    [Required] public string Name { get; init; }
    [Required] public string BrandId { get; init; }
    [Required] public string TypeId { get; init; }
    [Range(0.01, double.MaxValue)] public decimal Price { get; init; }
}

// Responses/ProductResponse.cs
public record ProductResponse
{
    public string Id { get; init; }
    public string Name { get; init; }
    public BrandResponse? Brand { get; init; }
    public TypeResponse? Type { get; init; }
    public decimal? Price { get; init; }
}
```

#### 6.3 Query 与 Handler

**查询定义**：

```csharp
// Queries/GetAllProductsQuery.cs
public record GetAllProductsQuery(CatalogSpecParams CatalogSpecParams) : IRequest<Pagination<ProductResponse>>;

// Queries/GetProductByIdQuery.cs
public record GetProductByIdQuery(string Id) : IRequest<ProductResponse>;
```

**查询处理器**：

```csharp
// Handlers/GetAllProductsHandler.cs
public class GetAllProductsHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsQuery, Pagination<ProductResponse>>
{
    public async Task<Pagination<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var productList = await productRepository.GetProductsAsync(request.CatalogSpecParams);
        return productList.ToResponse();
    }
}
```

> 其他查询类似，详见 `Catalog.Application/Queries/` 和 `Catalog.Application/Handlers/` 目录

#### 6.4 Command 与 Handler

**命令定义**：

```csharp
// Commands/CreateProductCommand.cs
public record CreateProductCommand : IRequest<ProductResponse>
{
    public string Name { get; init; }
    public string BrandId { get; init; }
    public string TypeId { get; init; }
    public decimal Price { get; init; }
}

// Commands/DeleteProductByIdCommand.cs
public record DeleteProductByIdCommand(string Id) : IRequest<bool>;
```

**命令处理器**：

```csharp
// Handlers/CreateProductCommandHandler.cs
public class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var brand = await productRepository.GetBrandsByIdAsync(request.BrandId)
            ?? throw new ApplicationException($"Brand {request.BrandId} not found");
        var type = await productRepository.GetTypesByIdAsync(request.TypeId)
            ?? throw new ApplicationException($"Type {request.TypeId} not found");

        var product = await productRepository.CreateProductAsync(request.ToEntity(brand, type));
        return product.ToResponse();
    }
}
```

---

### 7. API 层 (Catalog.API)

#### 7.1 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// 注册 MongoDB 序列化器
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// 注册配置与 MongoDB 客户端
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// 注册 MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(GetAllBrandsHandler).Assembly));

// 注册仓储
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ITypeRepository, TypeRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// 执行数据库种子数据初始化
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSettings>>());
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

#### 7.2 CatalogController

```csharp
[ApiController]
[Route("/api/v1/[controller]")]
public class CatalogController(IMediator mediator) : ControllerBase
{
    [HttpGet("GetAllProducts")]
    public async Task<IActionResult> GetProducts([FromQuery] CatalogSpecParams catalogSpecParams)
        => Ok(await mediator.Send(new GetAllProductsQuery(catalogSpecParams)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
        => Ok(await mediator.Send(new GetProductByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        => Ok(await mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto updateProductDto)
        => await mediator.Send(updateProductDto.ToCommand(id)) ? NoContent() : NotFound();

    [HttpDelete]
    public async Task<IActionResult> DeleteProduct(string id)
        => await mediator.Send(new DeleteProductByIdCommand(id)) ? NoContent() : NotFound();
}
```

---

### 8. Docker 配置

**Dockerfile** (`src/Services/Catalog/Catalog.API/Dockerfile`)：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Catalog/Catalog.API/Catalog.API.csproj", "src/Services/Catalog/Catalog.API/"]
RUN dotnet restore "src/Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
```

**docker-compose.yaml**（项目根目录）：

```yaml
services:
  catalog.db:
    image: mongo
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  catalog.api:
    image: catalog.api
    build:
      context: .
      dockerfile: src/Services/Catalog/Catalog.API/Dockerfile
    environment:
      - DatabaseSettings__ConnectionString=mongodb://catalog.db:27017
      - DatabaseSettings__DatabaseName=CatalogDb
    depends_on:
      - catalog.db
    ports:
      - "8080:8080"

volumes:
  mongo_data:
```

启动服务：
```bash
docker-compose up -d
```

---

### 9. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/v1/Catalog/GetAllProducts` | 获取产品列表（支持分页、筛选、排序） |
| GET | `/api/v1/Catalog/{id}` | 根据 ID 获取产品 |
| GET | `/api/v1/Catalog/productName/{name}` | 根据名称搜索产品 |
| GET | `/api/v1/Catalog/brand/{brand}` | 根据品牌获取产品 |
| GET | `/api/v1/Catalog/GetAllBrands` | 获取所有品牌 |
| GET | `/api/v1/Catalog/GetAllTypes` | 获取所有类型 |
| POST | `/api/v1/Catalog` | 创建产品 |
| PUT | `/api/v1/Catalog/{id}` | 更新产品 |
| DELETE | `/api/v1/Catalog?id={id}` | 删除产品 |

## 三、Basket 微服务

Basket 微服务负责购物车管理，采用整洁架构设计，使用 Redis 作为分布式缓存存储。

### 1. 项目结构

```
src/Services/Basket/
├── Basket.API/              # API 层 - 控制器、程序入口
├── Basket.Application/      # 应用层 - CQRS 命令与查询处理
├── Basket.Core/             # 核心层 - 实体、仓储接口
└── Basket.Infrastructure/   # 基础设施层 - 仓储实现、Redis 配置
```

### 2. 项目依赖关系

```
Basket.API → Basket.Application, Basket.Infrastructure
Basket.Infrastructure → Basket.Application
Basket.Application → Basket.Core
```

### 3. NuGet 包依赖

| 项目 | 包名 | 版本 |
|------|------|------|
| Basket.API | Microsoft.AspNetCore.OpenApi | 10.0.0 |
| Basket.Application | MediatR | 14.1.0 |
| Basket.Infrastructure | Microsoft.Extensions.Caching.StackExchangeRedis | 10.0.5 |

---

### 4. 核心层 (Basket.Core)

#### 4.1 实体类

```csharp
// Entities/ShoppingCart.cs
public class ShoppingCart
{
    public string UserName { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = [];
    public ShoppingCart(string userName) => UserName = userName;
}

// Entities/ShoppingCartItem.cs
public class ShoppingCartItem
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string ImageFile { get; set; }
}

// Entities/BasketCheckout.cs
public class BasketCheckout
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string CardNumber { get; set; }
    public string PaymentMethod { get; set; }
}
```

#### 4.2 仓储接口

```csharp
// Repositories/IBasketRepository.cs
public interface IBasketRepository
{
    Task<ShoppingCart> GetBasket(string userName);
    Task<ShoppingCart> UpdateBasket(ShoppingCart shoppingCart);
    Task DeleteBasket(string userName);
}
```

---

### 5. 基础设施层 (Basket.Infrastructure)

#### 5.1 缓存配置

```csharp
// Settings/CacheSettings.cs
public class CacheSettings
{
    public string ConnectionString { get; init; }
}
```

#### 5.2 仓储实现

```csharp
// Repositories/BasketRepository.cs
public class BasketRepository(IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task<ShoppingCart> UpdateBasket(ShoppingCart shoppingCart)
    {
        await cache.SetStringAsync(shoppingCart.UserName, JsonSerializer.Serialize(shoppingCart));
        return await GetBasket(shoppingCart.UserName);
    }

    public async Task DeleteBasket(string userName) => await cache.RemoveAsync(userName);
}
```

---

### 6. 应用层 (Basket.Application)

#### 6.1 DTOs 与 Responses

```csharp
// DTOs/BasketDto.cs
public record ShoppingCartDto(string UserName, List<ShoppingCartItemDto> Items, decimal TotalPrice);
public record ShoppingCartItemDto(string ProductId, string ProductName, string ImageFile, decimal Price, int Quantity);
public record CreateShoppingCartItemDto(string ProductId, string ProductName, string ImageFile, decimal Price, int Quantity);

// Responses/ShoppingCartItemResponse.cs
public record class ShoppingCartResponse
{
    public string UserName { get; init; }
    public IEnumerable<ShoppingCartItemResponse> Items { get; init; }
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}
```

#### 6.2 Query 与 Handler

```csharp
// Queries/GetBasketByUserNameQuery.cs
public record GetBasketByUserNameQuery(string UserName) : IRequest<ShoppingCartResponse>;

// Handlers/GetBasketByUserNameHandler.cs
public class GetBasketByUserNameHandler(IBasketRepository basketRepository)
    : IRequestHandler<GetBasketByUserNameQuery, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(GetBasketByUserNameQuery request, CancellationToken cancellationToken)
    {
        var shoppingCart = await basketRepository.GetBasket(request.UserName);
        return shoppingCart?.ToResponse() ?? new ShoppingCartResponse { Items = [] };
    }
}
```

#### 6.3 Command 与 Handler

```csharp
// Commands/CreateShoppingCartCommand.cs
public record CreateShoppingCartCommand(string UserName, List<CreateShoppingCartItemDto> Items) 
    : IRequest<ShoppingCartResponse>;

// Commands/DeleteBasketByUserNameCommand.cs
public record DeleteBasketByUserNameCommand(string UserName) : IRequest<Unit>;

// Handlers/CreateShoppingCartHandler.cs
public class CreateShoppingCartHandler(IBasketRepository basketRepository)
    : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
    {
        var updatedCart = await basketRepository.UpdateBasket(request.ToEntity());
        return updatedCart.ToResponse();
    }
}
```

---

### 7. API 层 (Basket.API)

#### 7.1 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateShoppingCartHandler).Assembly));

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(nameof(CacheSettings)));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection(nameof(CacheSettings)).GetValue<string>("ConnectionString");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

#### 7.2 BasketController

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class BasketController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userName}")]
    public async Task<IActionResult> GetBasket(string userName)
        => Ok(await mediator.Send(new GetBasketByUserNameQuery(userName)));

    [HttpPost]
    public async Task<IActionResult> CreateBasket([FromBody] CreateShoppingCartCommand command)
        => Ok(await mediator.Send(command));

    [HttpDelete("{userName}")]
    public async Task<IActionResult> DeleteBasket(string userName)
        => Ok(await mediator.Send(new DeleteBasketByUserNameCommand(userName)));
}
```

---

### 8. Docker 配置

**Dockerfile** (`src/Services/Basket/Basket.API/Dockerfile`)：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8020

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Basket/Basket.API/Basket.API.csproj", "src/Services/Basket/Basket.API/"]
RUN dotnet restore "src/Services/Basket/Basket.API/Basket.API.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Basket.API.dll"]
```

---

### 9. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/v1/Basket/{userName}` | 获取用户购物车 |
| POST | `/api/v1/Basket` | 创建/更新购物车 |
| DELETE | `/api/v1/Basket/{userName}` | 删除用户购物车 |

