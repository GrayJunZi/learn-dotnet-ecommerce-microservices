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

**docker-compose.yaml**（项目根目录）：

```yaml
services:
  basket.db:
    image: redis:alpine
    ports:
      - "6379:6379"

  basket.api:
    image: basket.api
    build:
      context: .
      dockerfile: src/Services/Basket/Basket.API/Dockerfile
    environment:
      - CacheSettings__ConnectionString=basket.db:6379
    depends_on:
      - basket.db
    ports:
      - "8001:8020"
```

启动服务：
```bash
docker-compose up -d
```

---

### 9. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/v1/Basket/{userName}` | 获取用户购物车 |
| POST | `/api/v1/Basket` | 创建/更新购物车 |
| DELETE | `/api/v1/Basket/{userName}` | 删除用户购物车 |

---

### 10. 集成 Discount 微服务（gRPC 调用）

Basket 微服务通过 gRPC 调用 Discount 微服务获取产品折扣信息，在创建购物车时自动应用折扣优惠。

#### 10.1 添加 NuGet 包依赖

在 `Basket.Application.csproj` 中添加 gRPC 相关包：

```xml
<ItemGroup>
  <PackageReference Include="Grpc.AspNetCore" Version="2.76.0" />
  <PackageReference Include="Grpc.Net.Client" Version="2.76.0" />
  <PackageReference Include="Grpc.Tools" Version="2.78.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

#### 10.2 添加 Proto 文件

在 `Basket.Application/Protos/discount.proto` 中定义 gRPC 服务契约：

```protobuf
syntax = "proto3";

option csharp_namespace = "Discount.Grpc.Protos";

service DiscountProtoService {
  rpc GetDiscount(GetDiscountRequest) returns (CouponModel);
}

message GetDiscountRequest { string productName = 1; }

message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}
```

在 `.csproj` 中引用（客户端模式）：

```xml
<ItemGroup>
  <Protobuf Include="Protos\discount.proto" GrpcServices="Client" />
</ItemGroup>
```

#### 10.3 创建 gRPC 配置与服务

```csharp
// Settings/GrpcSettings.cs
namespace Basket.Application.Settings;

public class GrpcSettings
{
    public string DiscountUrl { get; set; }
}
```

```csharp
// GrpcServices/DiscountGrpcService.cs
using Discount.Grpc.Protos;

namespace Basket.Application.GrpcServices;

public class DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient client)
{
    public async Task<CouponModel> GetDiscount(string productName)
    {
        var request = new GetDiscountRequest { ProductName = productName };
        return await client.GetDiscountAsync(request);
    }
}
```

#### 10.4 修改 Handler 应用折扣

```csharp
// Handlers/CreateShoppingCartHandler.cs
public class CreateShoppingCartHandler(
    IBasketRepository basketRepository,
    DiscountGrpcService discountGrpcService) : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request,
        CancellationToken cancellationToken)
    {
        // 为每个商品获取折扣并应用
        foreach (var item in request.Items)
        {
            var coupon = await discountGrpcService.GetDiscount(item.ProductName);
            item.Price -= coupon.Amount;
        }

        var shoppingCart = request.ToEntity();
        var updatedCart = await basketRepository.UpdateBasket(shoppingCart);
        return updatedCart.ToResponse();
    }
}
```

#### 10.5 注册 gRPC 客户端

```csharp
// Basket.API/Program.cs
using Basket.Application.GrpcServices;
using Basket.Application.Settings;
using Discount.Grpc.Protos;

// 注册 gRPC 配置
builder.Services.Configure<GrpcSettings>(
    builder.Configuration.GetSection(nameof(GrpcSettings)));

// 注册 gRPC 客户端
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>((sp, options) =>
{
    var grpcSettings = sp.GetRequiredService<IOptions<GrpcSettings>>().Value;
    options.Address = new Uri(grpcSettings.DiscountUrl);
});

builder.Services.AddScoped<DiscountGrpcService>();
```

#### 10.6 配置文件

**appsettings.json**：

```json
{
  "CacheSettings": {
    "ConnectionString": "localhost:6379"
  },
  "GrpcSettings": {
    "DiscountUrl": "http://localhost:8030"
  }
}
```

#### 10.7 服务调用流程

```
用户创建购物车
       ↓
CreateShoppingCartHandler
       ↓
遍历商品 → gRPC 调用 DiscountGrpcService.GetDiscount()
       ↓
商品价格 -= 折扣金额
       ↓
保存购物车到 Redis
```

#### 10.8 更新 Docker Compose

```yaml
services:
  basket.api:
    environment:
      - CacheSettings__ConnectionString=basket.db:6379
      - GrpcSettings__DiscountUrl=http://discount.api:8080
    depends_on:
      - basket.db
      - discount.api
```

---

## 四、Discount 微服务

Discount 微服务负责折扣优惠管理，采用整洁架构设计，使用 PostgreSQL 作为数据存储，并通过 gRPC 提供高性能服务调用。

### 1. 项目结构

```
src/Services/Discount/
├── Discount.API/              # API 层 - gRPC 服务、程序入口
├── Discount.Application/      # 应用层 - CQRS 命令与查询处理、Proto 定义
├── Discount.Core/             # 核心层 - 实体、仓储接口
└── Discount.Infrastructure/   # 基础设施层 - 仓储实现、数据库配置
```

### 2. 项目依赖关系

```
Discount.API → Discount.Application, Discount.Infrastructure
Discount.Infrastructure → Discount.Core
Discount.Application → Discount.Core
```

### 3. NuGet 包依赖

| 项目 | 包名 | 版本 |
|------|------|------|
| Discount.Application | MediatR | 14.1.0 |
| Discount.Application | Grpc.Tools | 2.78.0 |
| Discount.Infrastructure | Dapper | 2.1.72 |
| Discount.Infrastructure | Npgsql | 10.0.2 |
| Discount.Infrastructure | Grpc.AspNetCore | 2.76.0 |

---

### 4. 核心层 (Discount.Core)

#### 4.1 实体类

```csharp
// Entities/Coupon.cs
public class Coupon
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }
}
```

#### 4.2 仓储接口

```csharp
// Repositories/IDiscountRepository.cs
public interface IDiscountRepository
{
    Task<Coupon> GetDiscount(string productName);
    Task<bool> CreateDiscount(Coupon coupon);
    Task<bool> UpdateDiscount(Coupon coupon);
    Task<bool> DeleteDiscount(string productName);
}
```

---

### 5. 基础设施层 (Discount.Infrastructure)

#### 5.1 数据库配置

```csharp
// Settings/DatabaseSettings.cs
public class DatabaseSettings
{
    public string ConnectionString { get; set; }
}
```

#### 5.2 仓储实现（使用 Dapper）

```csharp
// Repositories/DiscountRepository.cs
public class DiscountRepository : IDiscountRepository
{
    private readonly string _connectionString;

    public DiscountRepository(IOptions<DatabaseSettings> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<Coupon> GetDiscount(string productName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(
            "SELECT * FROM Coupon WHERE ProductName = @ProductName", 
            new { ProductName = productName });
        
        return coupon ?? new Coupon { ProductName = "No Discount", Amount = 0 };
    }

    public async Task<bool> CreateDiscount(Coupon coupon)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(
            "INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
            coupon);
        return affected > 0;
    }
}
```

#### 5.3 数据库迁移扩展

```csharp
// Settings/DbExtensions.cs
public static class DbExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var databaseSettings = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        
        ApplyMigration(databaseSettings.ConnectionString);
        return host;
    }

    private static void ApplyMigration(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        
        using var command = new NpgsqlCommand { Connection = connection };
        command.CommandText = "CREATE TABLE Coupon (Id SERIAL PRIMARY KEY, ProductName VARCHAR(500), Description TEXT, Amount INT)";
        command.ExecuteNonQuery();
    }
}
```

---

### 6. 应用层 (Discount.Application)

#### 6.1 gRPC Proto 定义

```protobuf
// Protos/discount.proto
syntax = "proto3";

option csharp_namespace = "Discount.Grpc.Protos";

service DiscountProtoService {
  rpc GetDiscount(GetDiscountRequest) returns (CouponModel);
  rpc CreateDiscount(CreateDiscountRequest) returns (CouponModel);
  rpc UpdateDiscount(UpdateDiscountRequest) returns (CouponModel);
  rpc DeleteDiscount(DeleteDiscountRequest) returns (DeleteDiscountResponse);
}

message GetDiscountRequest { string productName = 1; }

message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}

message DeleteDiscountRequest { string productName = 1; }
message DeleteDiscountResponse { bool success = 1; }
```

#### 6.2 DTOs

```csharp
// DTOs/CouponDto.cs
public record CouponDto(int Id, string ProductName, string Description, int Amount);
```

#### 6.3 Query 与 Command

```csharp
// Queries/GetDiscountQuery.cs
public record GetDiscountQuery(string ProductName) : IRequest<CouponDto>;

// Commands/CreateDiscountCommand.cs
public record CreateDiscountCommand(string ProductName, string Description, int Amount) : IRequest<CouponDto>;

// Commands/UpdateDiscountCommand.cs
public record UpdateDiscountCommand(int Id, string ProductName, string Description, int Amount) : IRequest<CouponDto>;

// Commands/DeleteDiscountCommand.cs
public record DeleteDiscountCommand(string ProductName) : IRequest<bool>;
```

#### 6.4 Handler

```csharp
// Handlers/GetDiscountHandler.cs
public class GetDiscountHandler(IDiscountRepository discountRepository) 
    : IRequestHandler<GetDiscountQuery, CouponDto>
{
    public async Task<CouponDto> Handle(GetDiscountQuery request, CancellationToken cancellationToken)
    {
        var coupon = await discountRepository.GetDiscount(request.ProductName);
        return coupon.ToDto();
    }
}

// Handlers/CreateDiscountHandler.cs
public class CreateDiscountHandler(IDiscountRepository discountRepository)
    : IRequestHandler<CreateDiscountCommand, CouponDto>
{
    public async Task<CouponDto> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
    {
        var coupon = request.ToEntity();
        await discountRepository.CreateDiscount(coupon);
        return coupon.ToDto();
    }
}
```

---

### 7. API 层 (Discount.API)

#### 7.1 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateDiscountHandler).Assembly));

builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddGrpc();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(nameof(DatabaseSettings)));

var app = builder.Build();

app.MigrateDatabase();
app.UseRouting();
app.MapGrpcService<DiscountService>();

app.Run();
```

#### 7.2 gRPC Service

```csharp
// Services/DiscountService.cs
public class DiscountService(IMediator mediator) : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new GetDiscountQuery(request.ProductName));
        return result.ToModel();
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(request.Coupon.ToCreateCommand());
        return result.ToModel();
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(request.Coupon.ToUpdateCommand());
        return result.ToModel();
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new DeleteDiscountCommand(request.ProductName));
        return new DeleteDiscountResponse { Success = result };
    }
}
```

---

### 8. Docker 配置

**Dockerfile** (`src/Services/Discount/Discount.API/Dockerfile`)：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Discount/Discount.API/Discount.API.csproj", "src/Services/Discount/Discount.API/"]
RUN dotnet restore "src/Services/Discount/Discount.API/Discount.API.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discount.API.dll"]
```

**docker-compose.yaml**（项目根目录）：

```yaml
services:
  discount.db:
    image: postgres
    environment:
      - POSTGRES_USER=admin
      - POSTGRES_PASSWORD=admin123
      - POSTGRES_DB=DiscountDb
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  discount.api:
    image: discount.api
    build:
      context: .
      dockerfile: src/Services/Discount/Discount.API/Dockerfile
    environment:
      - DatabaseSettings__ConnectionString=Server=discount.db;Port=5432;Database=DiscountDb;User Id=admin;Password=admin123;
    depends_on:
      - discount.db
    ports:
      - "8002:8080"

volumes:
  postgres_data:
```

启动服务：
```bash
docker-compose up -d
```

---

### 9. gRPC 服务端点

| 方法 | 服务 | 说明 |
|------|------|------|
| GetDiscount | DiscountProtoService | 根据产品名称获取折扣 |
| CreateDiscount | DiscountProtoService | 创建折扣优惠 |
| UpdateDiscount | DiscountProtoService | 更新折扣优惠 |
| DeleteDiscount | DiscountProtoService | 删除折扣优惠 |

## 五、在 Basket 微服务中使用 Discount 微服务

Basket 微服务通过 gRPC 调用 Discount 微服务获取产品折扣信息，在创建购物车时自动应用折扣优惠。

### 1. 添加 NuGet 包依赖

在 `Basket.Application.csproj` 中添加 gRPC 相关包：

```xml
<ItemGroup>
  <PackageReference Include="Grpc.AspNetCore" Version="2.76.0" />
  <PackageReference Include="Grpc.Net.Client" Version="2.76.0" />
  <PackageReference Include="Grpc.Tools" Version="2.78.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

### 2. 添加 Proto 文件

在 `Basket.Application/Protos/discount.proto` 中定义 gRPC 服务契约（与 Discount 服务一致）：

```protobuf
syntax = "proto3";

option csharp_namespace = "Discount.Grpc.Protos";

service DiscountProtoService {
  rpc GetDiscount(GetDiscountRequest) returns (CouponModel);
  rpc CreateDiscount(CreateDiscountRequest) returns (CouponModel);
  rpc UpdateDiscount(UpdateDiscountRequest) returns (CouponModel);
  rpc DeleteDiscount(DeleteDiscountRequest) returns (DeleteDiscountResponse);
}

message GetDiscountRequest { string productName = 1; }

message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}
```

在 `.csproj` 中引用 Proto 文件（客户端模式）：

```xml
<ItemGroup>
  <Protobuf Include="Protos\discount.proto" GrpcServices="Client" />
</ItemGroup>
```

### 3. 创建 gRPC 配置

```csharp
// Settings/GrpcSettings.cs
namespace Basket.Application.Settings;

public class GrpcSettings
{
    public string DiscountUrl { get; set; }
}
```

### 4. 创建 gRPC 服务客户端

```csharp
// GrpcServices/DiscountGrpcService.cs
using Discount.Grpc.Protos;

namespace Basket.Application.GrpcServices;

public class DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient)
{
    public async Task<CouponModel> GetDiscount(string productName)
    {
        var discountRequest = new GetDiscountRequest { ProductName = productName };
        return await discountProtoServiceClient.GetDiscountAsync(discountRequest);
    }
}
```

### 5. 修改 DTO

```csharp
// DTOs/BasketDto.cs
public record CreateShoppingCartItemDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string ImageFile { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

### 6. 修改 Handler 应用折扣

```csharp
// Handlers/CreateShoppingCartHandler.cs
public class CreateShoppingCartHandler(
    IBasketRepository basketRepository,
    DiscountGrpcService discountGrpcService) : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request,
        CancellationToken cancellationToken)
    {
        // 为每个商品获取折扣并应用
        foreach (var item in request.Items)
        {
            var coupon = await discountGrpcService.GetDiscount(item.ProductName);
            item.Price -= coupon.Amount;
        }

        var shoppingCart = request.ToEntity();
        var updatedCart = await basketRepository.UpdateBasket(shoppingCart);
        return updatedCart.ToResponse();
    }
}
```

### 7. 在 Program.cs 注册服务

```csharp
// Basket.API/Program.cs
using Basket.Application.GrpcServices;
using Basket.Application.Settings;
using Discount.Grpc.Protos;

// 注册 gRPC 配置
builder.Services.Configure<GrpcSettings>(
    builder.Configuration.GetSection(nameof(GrpcSettings)));

// 注册 gRPC 客户端
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>((sp, options) =>
{
    var grpcSettings = sp.GetRequiredService<IOptions<GrpcSettings>>().Value;
    options.Address = new Uri(grpcSettings.DiscountUrl);
});

builder.Services.AddScoped<DiscountGrpcService>();
```

### 8. 配置文件

**appsettings.json**：

```json
{
  "CacheSettings": {
    "ConnectionString": "localhost:6379"
  },
  "GrpcSettings": {
    "DiscountUrl": "http://localhost:8030"
  }
}
```

### 9. 服务调用流程

```
用户创建购物车
       ↓
CreateShoppingCartHandler
       ↓
遍历购物车商品 → 调用 DiscountGrpcService.GetDiscount()
       ↓
Discount 微服务返回折扣金额
       ↓
商品价格 -= 折扣金额
       ↓
保存购物车到 Redis
```

### 10. 修改 Docker Compose 配置

**docker-compose.yaml**（项目根目录）：

```yaml
services:
  basket.api:
    image: basket.api
    build:
      context: .
      dockerfile: src/Services/Basket/Basket.API/Dockerfile
    environment:
      - CacheSettings__ConnectionString=basket.db:6379
      - GrpcSettings__DiscountUrl=http://discount.api:8080
    depends_on:
      - basket.db
      - discount.api
    ports:
      - "8001:8020"
```

## 六、Ordering 微服务

Ordering 微服务负责订单管理，采用整洁架构设计，使用 SQL Server 作为数据存储，实现完整的 CQRS 模式。

### 1. 项目结构

```
src/Services/Ordering/
├── Ordering.API/              # API 层 - 控制器、程序入口
├── Ordering.Application/      # 应用层 - CQRS 命令与查询处理
├── Ordering.Core/             # 核心层 - 实体、仓储接口
└── Ordering.Infrastructure/   # 基础设施层 - 仓储实现、数据库配置
```

### 2. 项目依赖关系

```
Ordering.API → Ordering.Application, Ordering.Infrastructure
Ordering.Infrastructure → Ordering.Application
Ordering.Application → Ordering.Core
```

### 3. NuGet 包依赖

| 项目 | 包名 | 版本 |
|------|------|------|
| Ordering.API | Microsoft.AspNetCore.OpenApi | 10.0.0 |
| Ordering.Application | FluentValidation | 11.11.0 |
| Ordering.Core | Microsoft.EntityFrameworkCore | 10.0.0 |
| Ordering.Infrastructure | Microsoft.EntityFrameworkCore.SqlServer | 10.0.0 |

---

### 4. 核心层 (Ordering.Core)

#### 4.1 实体基类

```csharp
// Entities/EntityBase.cs
public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
}
```

#### 4.2 订单实体

```csharp
// Entities/Order.cs
public class Order : EntityBase
{
    public string? UserName { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public string? AddressLine { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? CardName { get; set; }
    public string? CardNumber { get; set; }
    public string? CardExpiration { get; set; }
    public string? Cvv { get; set; }
    public int? PaymentMethod { get; set; }
}
```

#### 4.3 仓储接口

```csharp
// Repositories/IAsyncRepository.cs
public interface IAsyncRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// Repositories/IOrderRepository.cs
public interface IOrderRepository : IAsyncRepository<Order>
{
    Task<IReadOnlyList<Order>> GetOrdersByUserNameAsync(string userName);
}
```

---

### 5. 基础设施层 (Ordering.Infrastructure)

#### 5.1 数据库配置

```csharp
// Settings/DatabaseSettings.cs
public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
```

#### 5.2 DbContext 配置

```csharp
// Data/OrderContext.cs
public class OrderContext : DbContext
{
    public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        
        modelBuilder.Entity<Order>().Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");
            
        base.OnModelCreating(modelBuilder);
    }
}
```

#### 5.3 仓储实现

```csharp
// Repositories/RepositoryBase.cs
public class RepositoryBase<T> : IAsyncRepository<T> where T : EntityBase
{
    protected readonly OrderContext _dbContext;

    public RepositoryBase(OrderContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}

// Repositories/OrderRepository.cs
public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(OrderContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserNameAsync(string userName)
    {
        return await _dbContext.Orders
            .Where(o => o.UserName == userName)
            .ToListAsync();
    }
}
```

---

### 6. 应用层 (Ordering.Application)

#### 6.1 CQRS 抽象接口

```csharp
// Abstractions/ICommand.cs
public interface ICommand : IRequest<Result<int>>
{
}

// Abstractions/ICommandHandler.cs
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result<int>>
    where TCommand : ICommand
{
}

// Abstractions/IQuery.cs
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

// Abstractions/IQueryHandler.cs
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

#### 6.2 DTOs

```csharp
// DTOs/OrderingDto.cs
public record OrderingDto
{
    public int Id { get; init; }
    public string? UserName { get; init; }
    public decimal? TotalPrice { get; init; }
    public string? Name { get; init; }
    public string? EmailAddress { get; init; }
    public string? AddressLine { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? CardName { get; init; }
    public string? CardNumber { get; init; }
    public string? CardExpiration { get; init; }
    public string? Cvv { get; init; }
    public int? PaymentMethod { get; init; }
    public DateTime CreatedDate { get; init; }
}
```

#### 6.3 Query 与 Handler

**查询定义**：

```csharp
// Orders/GetOrders/GetOrderListQuery.cs
public record GetOrderListQuery : IQuery<IReadOnlyList<OrderingDto>>;
```

**查询处理器**：

```csharp
// Orders/GetOrders/GetOrderListHandler.cs
public class GetOrderListHandler : IQueryHandler<GetOrderListQuery, IReadOnlyList<OrderingDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderListHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderingDto>>> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync();
        var orderDtos = orders.Select(o => o.ToDto()).ToList();
        return Result<IReadOnlyList<OrderingDto>>.Success(orderDtos);
    }
}
```

#### 6.4 Command 与 Handler

**命令定义**：

```csharp
// Orders/CreateOrder/CreateOrderCommand.cs
public record CreateOrderCommand : ICommand
{
    public string? UserName { get; init; }
    public decimal? TotalPrice { get; init; }
    public string? Name { get; init; }
    public string? EmailAddress { get; init; }
    public string? AddressLine { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? CardName { get; init; }
    public string? CardNumber { get; init; }
    public string? CardExpiration { get; init; }
    public string? Cvv { get; init; }
    public int? PaymentMethod { get; init; }
}
```

**命令处理器**：

```csharp
// Orders/CreateOrder/CreateOrderHandler.cs
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            UserName = request.UserName,
            TotalPrice = request.TotalPrice,
            Name = request.Name,
            EmailAddress = request.EmailAddress,
            AddressLine = request.AddressLine,
            Country = request.Country,
            State = request.State,
            ZipCode = request.ZipCode,
            CardName = request.CardName,
            CardNumber = request.CardNumber,
            CardExpiration = request.CardExpiration,
            Cvv = request.Cvv,
            PaymentMethod = request.PaymentMethod
        };

        var createdOrder = await _orderRepository.AddAsync(order);
        return Result<int>.Success(createdOrder.Id);
    }
}
```

#### 6.5 验证实现

```csharp
// Validators/CreateOrderCommandValidator.cs
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过50个字符");

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("订单总价必须大于0");

        RuleFor(x => x.EmailAddress)
            .NotEmpty().WithMessage("邮箱地址不能为空")
            .EmailAddress().WithMessage("邮箱地址格式不正确");

        RuleFor(x => x.AddressLine)
            .NotEmpty().WithMessage("地址不能为空")
            .MaximumLength(200).WithMessage("地址长度不能超过200个字符");
    }
}
```

#### 6.6 异常处理

```csharp
// Exceptions/OrderNotFoundException.cs
public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(int orderId) 
        : base($"订单ID {orderId} 未找到")
    {
    }
}
```

#### 6.7 行为装饰器

```csharp
// Behaviors/ValidationCommandHandlerDecorator.cs
public class ValidationCommandHandlerDecorator<TCommand, TResponse> 
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand, TResponse> _inner;
    private readonly IValidator<TCommand> _validator;

    public ValidationCommandHandlerDecorator(
        ICommandHandler<TCommand, TResponse> inner,
        IValidator<TCommand> validator)
    {
        _inner = inner;
        _validator = validator;
    }

    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await _inner.Handle(request, cancellationToken);
    }
}
```

---

### 7. API 层 (Ordering.API)

#### 7.1 控制器实现

```csharp
// Controllers/OrderController.cs
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderingDto>>> GetOrders()
    {
        var query = new GetOrderListQuery();
        var result = await _mediator.Send(query);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetOrders), new { id = result.Value }, result.Value);
        }
        
        return BadRequest(result.Error);
    }
}
```

#### 7.2 依赖注入扩展

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));

        services.AddDbContext<OrderContext>((sp, options) =>
        {
            var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            options.UseSqlServer(databaseSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    sqlOptions.MigrationsAssembly("Ordering.Infrastructure");
                });
        });

        services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.Scan(scan => scan
            .FromAssemblies(typeof(ICommandHandler<>).Assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        
        return services;
    }
}
```

#### 7.3 数据库扩展

```csharp
// Extensions/DbExtension.cs
public static class DbExtension
{
    public static void MigrateDatabase<TContext>(this IApplicationBuilder app, 
        Action<TContext, IServiceProvider> seeder) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TContext>();
        
        context.Database.Migrate();
        seeder(context, services);
    }
}
```

#### 7.4 种子数据

```csharp
// Data/OrderContextSeed.cs
public class OrderContextSeed
{
    public static async Task SeedAsync(OrderContext context, ILogger<OrderContextSeed> logger)
    {
        if (!context.Orders.Any())
        {
            context.Orders.AddRange(GetPreconfiguredOrders());
            await context.SaveChangesAsync();
            logger.LogInformation("订单种子数据已插入数据库");
        }
    }

    private static IEnumerable<Order> GetPreconfiguredOrders()
    {
        return new List<Order>
        {
            new Order
            {
                UserName = "testuser",
                TotalPrice = 99.99m,
                Name = "张三",
                EmailAddress = "zhangsan@example.com",
                AddressLine = "北京市朝阳区",
                Country = "中国",
                State = "北京",
                ZipCode = "100000",
                CardName = "张三",
                CardNumber = "1234567890123456",
                CardExpiration = "12/25",
                Cvv = "123",
                PaymentMethod = 1
            }
        };
    }
}
```

#### 7.5 Program.cs 配置

```csharp
// Program.cs
using Ordering.API.Extensions;
using Ordering.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddOrderingServices(builder.Configuration);

var app = builder.Build();

app.MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetRequiredService<ILogger<OrderContextSeed>>();
    OrderContextSeed.SeedAsync(context, logger).Wait();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
```

---

### 8. 数据库迁移

Ordering 微服务使用 Entity Framework Core 进行数据库迁移管理：

```bash
# 创建迁移
dotnet ef migrations add InitialCreate --project src/Services/Ordering/Ordering.Infrastructure --startup-project src/Services/Ordering/Ordering.API

# 应用迁移
dotnet ef database update --project src/Services/Ordering/Ordering.Infrastructure --startup-project src/Services/Ordering/Ordering.API
```

**迁移文件位置**：`Ordering.Infrastructure/Migrations/`

---

### 9. 配置示例

#### 9.1 appsettings.json

```json
{
  "DatabaseSettings": {
    "ConnectionString": "Server=localhost;Database=OrderDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### 9.2 Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Ordering/Ordering.API/Ordering.API.csproj", "src/Services/Ordering/Ordering.API/"]
COPY ["src/Services/Ordering/Ordering.Application/Ordering.Application.csproj", "src/Services/Ordering/Ordering.Application/"]
COPY ["src/Services/Ordering/Ordering.Core/Ordering.Core.csproj", "src/Services/Ordering/Ordering.Core/"]
COPY ["src/Services/Ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "src/Services/Ordering/Ordering.Infrastructure/"]
RUN dotnet restore "src/Services/Ordering/Ordering.API/Ordering.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Ordering/Ordering.API"
RUN dotnet build "Ordering.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ordering.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]
```

## 七、在 Basket 微服务和 Ordering 微服务之间建立异步通信

为了实现 Basket 微服务和 Ordering 微服务之间的松耦合通信，我们采用事件驱动架构，通过 RabbitMQ 和 MassTransit 实现异步消息传递。

### 1. 创建基础设施 EventBus.Messages 项目

首先创建一个共享的消息契约项目，用于定义微服务之间通信的消息格式。

#### 1.1 项目结构

```
src/BuildingBlocks/
└── EventBus.Messages/
    ├── Common/                    # 通用消息基类
    ├── Events/                    # 事件定义
    └── Messages.csproj           # 项目文件
```

#### 1.2 EventBus.Messages.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" Version="8.3.0" />
  </ItemGroup>

</Project>
```

### 2. 创建基础集成事件

在 EventBus.Messages 项目中定义通用的集成事件基类和具体的事件类型。

#### 2.1 集成事件基类

```csharp
// Common/IntegrationBaseEvent.cs
using MassTransit;

namespace EventBus.Messages.Common;

public abstract class IntegrationBaseEvent : CorrelatedBy<Guid>
{
    public IntegrationBaseEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    public IntegrationBaseEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }

    public Guid Id { get; private set; }
    public DateTime CreationDate { get; private set; }
    public Guid CorrelationId { get; set; }
}
```

#### 2.2 BasketCheckout 事件

```csharp
// Events/BasketCheckoutEvent.cs
using EventBus.Messages.Common;

namespace EventBus.Messages.Events;

public class BasketCheckoutEvent : IntegrationBaseEvent
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    
    // 账单地址
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    
    // 支付信息
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string Expiration { get; set; }
    public string CVV { get; set; }
    public int PaymentMethod { get; set; }
}
```

### 3. 安装 MassTransit 组件

在 Basket.API 和 Ordering.API 项目中安装 MassTransit 相关组件。

#### 3.1 Basket.API 项目依赖

```xml
<!-- Basket.API.csproj -->
<ItemGroup>
  <PackageReference Include="MassTransit" Version="8.3.0" />
  <PackageReference Include="MassTransit.RabbitMQ" Version="8.3.0" />
  <PackageReference Include="MassTransit.Extensions.DependencyInjection" Version="8.3.0" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\EventBus.Messages\EventBus.Messages.csproj" />
</ItemGroup>
```

#### 3.2 Ordering.API 项目依赖

```xml
<!-- Ordering.API.csproj -->
<ItemGroup>
  <PackageReference Include="MassTransit" Version="8.3.0" />
  <PackageReference Include="MassTransit.RabbitMQ" Version="8.3.0" />
  <PackageReference Include="MassTransit.Extensions.DependencyInjection" Version="8.3.0" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\EventBus.Messages\EventBus.Messages.csproj" />
</ItemGroup>
```

### 4. 创建 BasketCheckout 命令与处理器

在 Basket.API 项目中创建处理结账流程的命令和处理器。

#### 4.1 BasketCheckout 命令

```csharp
// Application/Features/BasketCheckout/Commands/CheckoutBasketCommand.cs
using MediatR;

namespace Basket.API.Application.Features.BasketCheckout.Commands;

public class CheckoutBasketCommand : IRequest<int>
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    
    // 账单地址
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    
    // 支付信息
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string Expiration { get; set; }
    public string CVV { get; set; }
    public int PaymentMethod { get; set; }
}
```

#### 4.2 BasketCheckout 命令处理器

```csharp
// Application/Features/BasketCheckout/Commands/CheckoutBasketCommandHandler.cs
using Basket.API.Application.Contracts.Infrastructure;
using Basket.API.Application.Contracts.Persistence;
using Basket.API.Application.Models;
using EventBus.Messages.Events;
using MassTransit;
using MediatR;

namespace Basket.API.Application.Features.BasketCheckout.Commands;

public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand, int>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public CheckoutBasketCommandHandler(
        IBasketRepository basketRepository,
        IPublishEndpoint publishEndpoint,
        IMapper mapper)
    {
        _basketRepository = basketRepository;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    public async Task<int> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        // 1. 获取购物车
        var basket = await _basketRepository.GetBasketAsync(request.UserName);
        if (basket == null)
        {
            throw new Exception("购物车不存在");
        }

        // 2. 创建结账事件
        var eventMessage = _mapper.Map<BasketCheckoutEvent>(request);
        eventMessage.TotalPrice = basket.TotalPrice;

        // 3. 发送事件到消息队列
        await _publishEndpoint.Publish(eventMessage, cancellationToken);

        // 4. 清空购物车
        await _basketRepository.DeleteBasketAsync(request.UserName);

        return 1;
    }
}
```

### 5. 创建 BasketCheckout 控制器方法

在 Basket.API 控制器中添加处理结账的 API 端点。

#### 5.1 BasketCheckout 控制器

```csharp
// Controllers/BasketCheckoutController.cs
using Basket.API.Application.Features.BasketCheckout.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketCheckoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public BasketCheckoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost(Name = "CheckoutBasket")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<int>> CheckoutBasket([FromBody] CheckoutBasketCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

### 6. 创建 BasketOrdering Consumer

在 Ordering.API 项目中创建消费者来处理来自 Basket 微服务的结账事件。

#### 6.1 BasketCheckout Consumer

```csharp
// Application/Features/Orders/EventHandlers/BasketCheckoutConsumer.cs
using EventBus.Messages.Events;
using MassTransit;
using Ordering.API.Application.Contracts.Infrastructure;
using Ordering.API.Application.Contracts.Persistence;
using Ordering.API.Application.Models;

namespace Ordering.API.Application.Features.Orders.EventHandlers;

public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<BasketCheckoutConsumer> _logger;

    public BasketCheckoutConsumer(
        IOrderRepository orderRepository,
        IMapper mapper,
        IEmailService emailService,
        ILogger<BasketCheckoutConsumer> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        try
        {
            var message = context.Message;
            
            // 1. 创建订单
            var order = _mapper.Map<Order>(message);
            await _orderRepository.AddAsync(order);

            // 2. 发送确认邮件
            var email = new Email
            {
                To = message.EmailAddress,
                Subject = "订单确认",
                Body = $"尊敬的 {message.FirstName} {message.LastName}，您的订单已成功创建。订单总金额：{message.TotalPrice}"
            };

            await _emailService.SendEmail(email);

            _logger.LogInformation($"BasketCheckoutEvent 消费成功。订单ID：{order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理 BasketCheckoutEvent 时发生错误: {ex.Message}");
            throw;
        }
    }
}
```

#### 6.2 配置 MassTransit 消费者

在 Ordering.API 的 Program.cs 中配置 MassTransit：

```csharp
// Program.cs
using MassTransit;
using Ordering.API.Application.Features.Orders.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// 配置 MassTransit
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<BasketCheckoutConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        
        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
        {
            c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
        });
    });
});

// 其他配置...
```

#### 6.3 事件总线常量

```csharp
// Common/EventBusConstants.cs
namespace Ordering.API.Application.Common;

public static class EventBusConstants
{
    public const string BasketCheckoutQueue = "basketcheckout-queue";
}
```

### 7. 配置 RabbitMQ 连接

在 appsettings.json 中添加 RabbitMQ 配置：

```json
{
  "EventBusSettings": {
    "HostAddress": "rabbitmq://localhost:5672"
  }
}
```

### 8. Docker 配置

为了支持容器化部署，我们需要为 Basket.API 和 Ordering.API 配置 Dockerfile，并更新 docker-compose.yml 文件以包含所有必要的服务。

#### 8.1 Basket.API Dockerfile

**Dockerfile** (`src/Services/Basket/Basket.API/Dockerfile`)：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Basket/Basket.API/Basket.API.csproj", "src/Services/Basket/Basket.API/"]
COPY ["src/Services/Basket/Basket.Application/Basket.Application.csproj", "src/Services/Basket/Basket.Application/"]
COPY ["src/Services/Basket/Basket.Core/Basket.Core.csproj", "src/Services/Basket/Basket.Core/"]
COPY ["src/Services/Basket/Basket.Infrastructure/Basket.Infrastructure.csproj", "src/Services/Basket/Basket.Infrastructure/"]
COPY ["src/BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus.Messages/"]
RUN dotnet restore "src/Services/Basket/Basket.API/Basket.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Basket/Basket.API"
RUN dotnet build "Basket.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Basket.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Basket.API.dll"]
```

#### 8.2 Ordering.API Dockerfile

**Dockerfile** (`src/Services/Ordering/Ordering.API/Dockerfile`)：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Ordering/Ordering.API/Ordering.API.csproj", "src/Services/Ordering/Ordering.API/"]
COPY ["src/Services/Ordering/Ordering.Application/Ordering.Application.csproj", "src/Services/Ordering/Ordering.Application/"]
COPY ["src/Services/Ordering/Ordering.Core/Ordering.Core.csproj", "src/Services/Ordering/Ordering.Core/"]
COPY ["src/Services/Ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "src/Services/Ordering/Ordering.Infrastructure/"]
COPY ["src/BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus.Messages/"]
RUN dotnet restore "src/Services/Ordering/Ordering.API/Ordering.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Ordering/Ordering.API"
RUN dotnet build "Ordering.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ordering.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]
```

#### 8.3 更新 docker-compose.yml

**docker-compose.yml**（项目根目录）：

```yaml
version: '3.8'

services:
  # RabbitMQ 消息队列
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
```

#### 8.4 启动所有服务

使用以下命令启动所有微服务和基础设施：

```bash
docker-compose up -d
```