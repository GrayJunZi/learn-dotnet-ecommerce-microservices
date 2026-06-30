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

## 八、实现 SAGA 模式

### 1. 介绍

在分布式系统中，传统的两阶段提交（2PC）事务难以实现，因为涉及多个独立的微服务和数据库。SAGA 模式提供了一种替代方案，通过将长事务拆分为多个本地事务来实现最终一致性。

本章节将实现基于 Outbox 模式的 SAGA 解决方案，确保消息传递的可靠性和事务一致性。

### 2. SAGA 模式概述

**SAGA 模式的核心概念：**

| 特性 | 说明 |
|------|------|
| **本地事务** | 每个服务只负责自己的本地事务 |
| **事件驱动** | 通过事件触发后续步骤 |
| **补偿机制** | 失败时执行补偿操作回滚 |
| **最终一致性** | 保证数据最终达到一致状态 |

**SAGA 协调方式：**
- **编排式（Choreography）**：每个服务知道自己需要触发哪些后续服务
- **编排式（Orchestration）**：由专门的协调器控制整个流程

本项目采用**编排式 + Outbox 模式**的组合方案。

### 3. 创建 Outbox Message 实体

Outbox 模式确保消息与数据库事务的原子性，避免消息丢失或重复发送。

```csharp
// Ordering.Core/Entities/OutboxMessage.cs
using System.Text.Json;

namespace Ordering.Core.Entities;

public class OutboxMessage : EntityBase
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ProcessedDate { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public static OutboxMessage Create<T>(T message)
    {
        return new OutboxMessage
        {
            Type = typeof(T).AssemblyQualifiedName ?? string.Empty,
            Content = JsonSerializer.Serialize(message),
            RetryCount = 0
        };
    }

    public object? Deserialize()
    {
        var type = Type.GetType(Type);
        if (type == null) return null;
        
        return JsonSerializer.Deserialize(Content, type);
    }
}
```

### 4. 创建 Order Status 订单状态

为订单实体添加状态字段，用于跟踪 SAGA 流程的执行状态。

```csharp
// Ordering.Core/Enums/OrderStatus.cs
namespace Ordering.Core.Enums;

public enum OrderStatus
{
    Pending = 1,
    Created = 2,
    PaymentCompleted = 3,
    ShippingCompleted = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7
}
```

更新 Order 实体添加状态字段：

```csharp
// Ordering.Core/Entities/Order.cs
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
    
    // SAGA 相关字段
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? PaymentTransactionId { get; set; }
}
```

### 5. 扩展 Order Context

在 OrderContext 中添加 OutboxMessage DbSet 并配置实体映射。

```csharp
// Ordering.Infrastructure/Data/OrderContext.cs
public class OrderContext : DbContext
{
    public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        
        modelBuilder.Entity<Order>().Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>().Property(o => o.Status)
            .HasConversion<string>();

        // OutboxMessage 配置
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
            entity.HasIndex(e => e.ProcessedDate);
        });

        base.OnModelCreating(modelBuilder);
    }
}
```

### 6. 扩展 Order Repository

添加 OutboxMessage 相关的仓储方法。

```csharp
// Ordering.Core/Repositories/IOutboxRepository.cs
namespace Ordering.Core.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100);
    Task MarkAsProcessedAsync(int messageId);
    Task UpdateRetryCountAndErrorAsync(int messageId, string errorMessage);
}
```

```csharp
// Ordering.Infrastructure/Repositories/OutboxRepository.cs
namespace Ordering.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly OrderContext _dbContext;

    public OutboxRepository(OrderContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage message)
    {
        await _dbContext.OutboxMessages.AddAsync(message);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedDate == null)
            .OrderBy(m => m.CreatedDate)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkAsProcessedAsync(int messageId)
    {
        var message = await _dbContext.OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.ProcessedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateRetryCountAndErrorAsync(int messageId, string errorMessage)
    {
        var message = await _dbContext.OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.RetryCount++;
            message.ErrorMessage = errorMessage;
            await _dbContext.SaveChangesAsync();
        }
    }
}
```

### 7. 扩展 Order Creation Handler

修改 CreateOrderHandler，集成 Outbox 模式，将消息写入 Outbox 表而不是直接发布。

```csharp
// Ordering.Application/Orders/CreateOrder/CreateOrderHandler.cs
using EventBus.Messages.Events;

public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IOutboxRepository outboxRepository)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. 创建订单实体
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
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatus.Created
        };

        // 2. 创建 Outbox 消息
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserName = order.UserName,
            TotalPrice = order.TotalPrice ?? 0,
            EmailAddress = order.EmailAddress ?? string.Empty
        };

        var outboxMessage = OutboxMessage.Create(orderCreatedEvent);

        // 3. 在同一事务中保存订单和消息
        await _orderRepository.AddAsync(order);
        await _outboxRepository.AddAsync(outboxMessage);
        
        // 使用 OrderContext 的 SaveChangesAsync 确保原子性
        await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(order.Id);
    }
}
```

创建 OrderCreatedEvent 事件：

```csharp
// EventBus.Messages/Events/OrderCreatedEvent.cs
using EventBus.Messages.Common;

namespace EventBus.Messages.Events;

public class OrderCreatedEvent : IntegrationBaseEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
}
```

### 8. 创建 Outbox Message Dispatcher Service

创建后台服务定期从 Outbox 表读取消息并发布到 RabbitMQ。

```csharp
// Ordering.API/Services/OutboxMessageDispatcher.cs
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Core.Repositories;

namespace Ordering.API.Services;

public class OutboxMessageDispatcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageDispatcher> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 5;

    public OutboxMessageDispatcher(
        IServiceProvider serviceProvider,
        ILogger<OutboxMessageDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Message Dispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Message Dispatcher stopping");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(10);
        
        foreach (var message in messages)
        {
            try
            {
                if (message.RetryCount >= MaxRetryCount)
                {
                    _logger.LogWarning("Message {MessageId} has exceeded max retry count", message.Id);
                    await outboxRepository.UpdateRetryCountAndErrorAsync(message.Id, "Max retry count exceeded");
                    continue;
                }

                var eventMessage = message.Deserialize();
                if (eventMessage != null)
                {
                    await publishEndpoint.Publish(eventMessage, cancellationToken);
                    await outboxRepository.MarkAsProcessedAsync(message.Id);
                    _logger.LogInformation("Outbox message {MessageId} processed successfully", message.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                await outboxRepository.UpdateRetryCountAndErrorAsync(message.Id, ex.Message);
            }
        }
    }
}
```

### 9. 配置 Program.cs

在 Program.cs 中注册 Outbox Dispatcher 服务和相关依赖。

```csharp
// Ordering.API/Program.cs
using MassTransit;
using Ordering.API.Services;
using Ordering.Core.Repositories;
using Ordering.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 注册 Ordering 服务
builder.Services.AddOrderingServices(builder.Configuration);

// 注册 Outbox Repository
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

// 注册 Outbox Message Dispatcher
builder.Services.AddHostedService<OutboxMessageDispatcher>();

// 配置 MassTransit
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

// 数据库迁移
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
app.MapControllers();
app.Run();
```

### 10. EF Migration 和 Docker Build

**创建 OutboxMessage 表迁移：**

```bash
# 创建迁移
dotnet ef migrations add OutboxMessageTable --project src/Services/Ordering/Ordering.Infrastructure --startup-project src/Services/Ordering/Ordering.API

# 应用迁移
dotnet ef database update --project src/Services/Ordering/Ordering.Infrastructure --startup-project src/Services/Ordering/Ordering.API
```

**Docker Compose 配置：**

```yaml
# docker-compose.yml
services:
  ordering.db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=YourStrong!Passw0rd
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  ordering.api:
    image: ordering.api
    build:
      context: .
      dockerfile: src/Services/Ordering/Ordering.API/Dockerfile
    environment:
      - DatabaseSettings__ConnectionString=Server=ordering.db;Database=OrderDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;
      - EventBusSettings__HostAddress=rabbitmq://rabbitmq:5672
    depends_on:
      - ordering.db
      - rabbitmq
    ports:
      - "8003:8080"

  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"

volumes:
  sqlserver_data:
```

**启动服务：**

```bash
docker-compose up -d
```

### 11. Outbox Table Demo

**OutboxMessages 表结构：**

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| Type | varchar(500) | 事件类型全称 |
| Content | text | 序列化后的事件内容 |
| ProcessedDate | datetime | 处理完成时间 |
| ErrorMessage | nvarchar(max) | 错误信息 |
| RetryCount | int | 重试次数 |
| CreatedDate | datetime | 创建时间 |
| LastModifiedDate | datetime | 最后修改时间 |

**典型数据示例：**

```sql
SELECT * FROM OutboxMessages;

-- 结果示例
-- Id | Type                                      | Content                                                                 | ProcessedDate       | ErrorMessage | RetryCount | CreatedDate
-- 1  | EventBus.Messages.Events.OrderCreatedEvent | {"OrderId":1,"UserName":"testuser","TotalPrice":99.99,"EmailAddress":"test@example.com"} | 2024-01-15 10:30:00 | NULL        | 0          | 2024-01-15 10:29:55
```

**工作流程：**

```
1. 用户创建订单
       ↓
2. CreateOrderHandler 创建 Order 和 OutboxMessage
       ↓
3. 同一事务保存到数据库
       ↓
4. OutboxMessageDispatcher 轮询未处理消息
       ↓
5. 反序列化消息并发布到 RabbitMQ
       ↓
6. 标记消息为已处理
       ↓
7. 下游服务消费消息
```

**优势：**

- **原子性**：订单创建和消息写入在同一事务中
- **可靠性**：即使服务崩溃，消息也不会丢失
- **最终一致性**：通过重试机制确保消息最终被处理
- **可追溯性**：Outbox 表提供完整的消息历史记录

## 九、Payment 微服务

Payment 微服务作为 SAGA 模式中的关键参与者，负责处理订单支付流程。它通过 RabbitMQ 监听订单创建事件，执行支付逻辑，并根据支付结果发布相应事件，驱动订单状态的流转。

### 1. 介绍

Payment 微服务在整个 SAGA 流程中扮演"支付处理器"的角色：

- **监听订单创建事件** - 当 Ordering 微服务通过 OutboxDispatcher 发布 `OrderCreatedEvent` 后进行支付处理
- **执行支付逻辑** - 根据订单总金额判断支付成功或失败
- **发布支付结果** - 成功发布 `PaymentCompletedEvent`，失败发布 `PaymentFailedEvent`
- **驱动状态流转** - 下游消费者根据支付结果更新订单状态

**SAGA 支付流程：**

```
Basket → Checkout → OrderCreated (Outbox) → Payment → PaymentCompleted/PaymentFailed → Order Status Update
```

### 2. 创建 Payment 微服务

Payment 微服务是一个独立的最小化 ASP.NET Core Web API 项目，不需要传统分层结构，直接使用 MassTransit 消费者处理消息。

**项目结构：**

```
src/Services/Payment/
└── Payment.API/
    ├── Consumer/
    │   └── OrderCreatedConsumer.cs    # 订单创建事件消费者
    ├── Properties/
    │   └── launchSettings.json
    ├── Dockerfile
    ├── Payment.API.csproj
    ├── Payment.API.http
    ├── Program.cs
    ├── appsettings.Development.json
    └── appsettings.json
```

### 3. 安装 NuGet 包

在 `Payment.API.csproj` 中添加必要的 NuGet 依赖：

```xml
<!-- Payment.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" Version="9.1.1" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="9.1.1" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.8" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Infrastructure\EventBus.Messages\EventBus.Messages.csproj" />
  </ItemGroup>

</Project>
```

| 包名 | 用途 |
|------|------|
| MassTransit | 分布式消息总线框架 |
| MassTransit.RabbitMQ | RabbitMQ 传输层支持 |
| EventBus.Messages | 共享事件消息契约 |
| Microsoft.AspNetCore.OpenApi | OpenAPI 支持 |

### 4. 创建 Order Created Consumer

`OrderCreatedConsumer` 是 Payment 微服务的核心组件，负责处理订单创建事件中的支付逻辑。

```csharp
// Consumer/OrderCreatedConsumer.cs
using EventBus.Messages.Events;
using MassTransit;

namespace Payment.API.Consumer;

public class OrderCreatedConsumer(
    IPublishEndpoint publishEndpoint,
    ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing Payment or Order Id: {OrderId}", message.Id);

        await Task.Delay(1000);

        if (message.TotalPrice > 0)
        {
            var completedEvent = new PaymentCompletedEvent
            {
                OrderId = message.Id,
                CorrelationId = context.CorrelationId.Value,
            };

            await publishEndpoint.Publish(completedEvent);
            logger.LogInformation(
                "Payment successfully completed for Order Id: {OrderId} and Correlation Id: {Correlation}",
                message.Id, context.CorrelationId);
        }
        else
        {
            var failedEvent = new PaymentFailedEvent
            {
                OrderId = message.Id,
                CorrelationId = context.CorrelationId.Value,
                Reason = "Total price was zero or negative.",
            };

            await publishEndpoint.Publish(failedEvent);
            logger.LogWarning(
                "Payment failed for Order Id: {OrderId} and Correlation Id: {Correlation}",
                message.Id, context.CorrelationId);
        }
    }
}
```

**核心逻辑说明：**

| 步骤 | 操作 | 说明 |
|------|------|------|
| 1 | 接收消息 | 通过 MassTransit 接收 `OrderCreatedEvent` |
| 2 | 模拟支付 | 使用 `Task.Delay` 模拟支付处理延迟 |
| 3 | 判断结果 | `TotalPrice > 0` 则支付成功，否则失败 |
| 4 | 发布事件 | 根据结果发布 `PaymentCompletedEvent` 或 `PaymentFailedEvent` |
| 5 | 传递 CorrelationId | 保持 SAGA 追踪链的完整性 |

### 5. 配置 Program.cs

在 Payment 微服务的 `Program.cs` 中配置 MassTransit 连接 RabbitMQ 并注册消费者。

```csharp
// Program.cs
using EventBus.Messages.Common;
using MassTransit;
using Payment.API.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstants.OrderCreatedQueue, c =>
        {
            c.ConfigureConsumer<OrderCreatedConsumer>(ctx);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
```

**配置文件 `appsettings.json`：**

```json
{
  "EventBusSettings": {
    "HostAddress": "amqp://guest:guest@localhost:5672"
  }
}
```

### 6. Payment Completed Consumer（在 Ordering 微服务中）

Payment 微服务发布 `PaymentCompletedEvent` 后，Ordering 微服务需要监听并处理此事件，将订单状态更新为"已支付"。

**PaymentCompletedEvent 事件定义：**

```csharp
// EventBus.Messages/Events/PaymentCompletedEvent.cs
namespace EventBus.Messages.Events;

public class PaymentCompletedEvent : BaseIntegrationEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
```

**PaymentCompletedConsumer 消费者：**

```csharp
// Ordering.Application/EventBusConsumer/PaymentCompletedConsumer.cs
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.Repositories;

namespace Ordering.Application.EventBusConsumer;

public class PaymentCompletedConsumer(
    IOrderRepository orderRepository,
    ILogger<PaymentCompletedConsumer> logger) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order not found for Id: {OrderId} and CorrelationId: {CorrelationId}",
                context.Message.OrderId, context.CorrelationId);
            return;
        }

        order.Status = Core.Enums.OrderStatus.Paid;
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Order Id {OrderId} marked as Paid", context.Message.OrderId);
    }
}
```

### 7. Payment Failed Consumer（在 Ordering 微服务中）

Payment 微服务支付失败后发布 `PaymentFailedEvent`，Ordering 微服务监听此事件将订单状态更新为"失败"。

**PaymentFailedEvent 事件定义：**

```csharp
// EventBus.Messages/Events/PaymentFailedEvent.cs
namespace EventBus.Messages.Events;

public class PaymentFailedEvent : BaseIntegrationEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; }
    public string Reason { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
```

**PaymentFailedConsumer 消费者：**

```csharp
// Ordering.Application/EventBusConsumer/PaymentFailedConsumer.cs
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.Repositories;

namespace Ordering.Application.EventBusConsumer;

public class PaymentFailedConsumer(
    IOrderRepository orderRepository,
    ILogger<PaymentFailedConsumer> logger) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order not found for Id: {OrderId} and CorrelationId: {CorrelationId}",
                context.Message.OrderId, context.CorrelationId);
            return;
        }

        order.Status = Core.Enums.OrderStatus.Failed;
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Payment failed for Order Id {OrderId}, Reason: {Reason}", context.Message.OrderId,
            context.Message.Reason);
    }
}
```

### 8. 配置 Ordering Program.cs

在 Ordering.API 的 `Program.cs` 中注册 Payment 相关的消费者，使 Ordering 微服务能够接收支付完成和支付失败事件。

```csharp
// Ordering.API/Program.cs
using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.Extensions;
using Ordering.Application.Dispatcher;
using Ordering.Application.EventBusConsumer;
using Ordering.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddOrderingServices(builder.Configuration);

builder.Services.AddHostedService<OutboxMessageDispatcher>();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<BasketOrderingConsumer>();
    configure.AddConsumer<PaymentCompletedConsumer>();
    configure.AddConsumer<PaymentFailedConsumer>();
    configure.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue,
            c => { c.ConfigureConsumer<BasketOrderingConsumer>(ctx); });

        cfg.ReceiveEndpoint(EventBusConstants.PaymentCompletedQueue,
            c => { c.ConfigureConsumer<PaymentCompletedConsumer>(ctx); });

        cfg.ReceiveEndpoint(EventBusConstants.PaymentFailedQueue,
            c => { c.ConfigureConsumer<PaymentFailedConsumer>(ctx); });
    });
});

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

**EventBusConstants 常量定义：**

```csharp
// EventBus.Messages/Common/EventBusConstants.cs
namespace EventBus.Messages.Common;

public class EventBusConstants
{
    public const string BasketCheckoutQueue = "basket-checkout-queue";
    public const string OrderCreatedQueue = "order-created-queue";
    public const string PaymentCompletedQueue = "payment-completed-queue";
    public const string PaymentFailedQueue = "payment-failed-queue";
}
```

**队列与消费者对应关系：**

| 队列 | 消费者 | 微服务 | 方向 |
|------|--------|--------|------|
| basket-checkout-queue | BasketOrderingConsumer | Ordering | 接收 Basket 结账事件 |
| order-created-queue | OrderCreatedConsumer | Payment | 接收订单创建事件 |
| payment-completed-queue | PaymentCompletedConsumer | Ordering | 接收支付完成事件 |
| payment-failed-queue | PaymentFailedConsumer | Ordering | 接收支付失败事件 |

### 9. Docker 配置

**Payment.API Dockerfile：**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Payment/Payment.API/Payment.API.csproj", "src/Services/Payment/Payment.API/"]
COPY ["src/Infrastructure/EventBus.Messages/EventBus.Messages.csproj", "src/Infrastructure/EventBus.Messages/"]
RUN dotnet restore "src/Services/Payment/Payment.API/Payment.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Payment/Payment.API"
RUN dotnet build "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
```

**Docker Compose 配置（Payment 服务）：**

```yaml
# docker-compose.yml
services:
  # Payment 微服务
  payment.api:
    image: payment.api
    build:
      context: .
      dockerfile: src/Services/Payment/Payment.API/Dockerfile
    environment:
      - EventBusSettings__HostAddress=amqp://guest:guest@rabbitmq:5672
    depends_on:
      - rabbitmq
    ports:
      - "8004:8080"
```

### 10. Pay 服务常见问题修复

开发过程中需要注意以下问题：

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 消费者未接收消息 | `AddMassTransit` 在 `builder.Build()` 之后调用 | 将 MassTransit 配置移到 `builder.Build()` 之前 |
| CorrelationId 丢失 | 发布事件时未传递 CorrelationId | 从 `context.CorrelationId.Value` 传递到事件 |
| 订单状态未更新 | Ordering 未注册 Payment 消费者 | 在 Ordering Program.cs 中注册 `PaymentCompletedConsumer` 和 `PaymentFailedConsumer` |
| 队列绑定失败 | 队列名称不匹配 | 确保 EventBusConstants 与 ReceiveEndpoint 名称一致 |

### 11. SAGA Outbox Pattern Demo

**完整 SAGA 流程演示：**

```
┌─────────────┐     ┌──────────────┐     ┌───────────────┐     ┌──────────────┐
│   Basket    │     │   Ordering   │     │   Payment     │     │   Ordering   │
│   (Redis)   │     │  (SQL Server)│     │   (微服务)    │     │  (Consumer)  │
└──────┬──────┘     └──────┬───────┘     └──────┬────────┘     └──────┬───────┘
       │                   │                    │                     │
       │ 1. BasketCheckout │                    │                     │
       │──────────────────>│                    │                     │
       │                   │                    │                     │
       │                   │ 2. CreateOrder     │                     │
       │                   │    + OutboxMessage │                     │
       │                   │                    │                     │
       │                   │ 3. OutboxDispatcher│                     │
       │                   │    → RabbitMQ      │                     │
       │                   │───────────────────>│                     │
       │                   │                    │                     │
       │                   │                    │ 4. OrderCreated     │
       │                   │                    │    Consumer         │
       │                   │                    │    (支付处理)        │
       │                   │                    │                     │
       │                   │                    │ 5a. PaymentCompleted│
       │                   │                    │────────────────────>│
       │                   │                    │                     │
       │                   │                    │ 5b. PaymentFailed   │
       │                   │                    │────────────────────>│
       │                   │                    │                     │
       │                   │                    │           6. Update │
       │                   │                    │           Order     │
       │                   │                    │           Status    │
```

**订单状态流转：**

```
Pending → Created → Paid (支付成功)
                  → Failed (支付失败)
```

**验证步骤：**

1. 启动所有服务（RabbitMQ、Basket、Ordering、Payment）：
   ```bash
   docker-compose up -d
   ```

2. 访问 RabbitMQ 管理界面查看队列：
   ```
   http://localhost:15672 (guest/guest)
   ```

3. 通过 Basket API 发起结账请求，触发完整 SAGA 流程

4. 在 SQL Server 中查询订单状态变化：
   ```sql
   SELECT Id, UserName, TotalPrice, Status, CreatedDate 
   FROM Orders 
   ORDER BY CreatedDate DESC;
   ```

5. 在 SQL Server 中查询 OutboxMessages 消息处理状态：
   ```sql
   SELECT Id, Type, ProcessedDate, RetryCount, ErrorMessage 
   FROM OutboxMessages 
   ORDER BY CreatedDate DESC;
   ```

**关键设计优势：**

- **原子性保证**：Outbox 模式确保订单创建和消息写入在同一数据库事务中
- **消息可靠性**：即使 Payment 或 Ordering 服务临时不可用，消息不会丢失
- **状态可追溯**：通过 OutboxMessages 表和订单 Status 字段完整追踪流程
- **松耦合通信**：Basket、Ordering、Payment 三个微服务通过 RabbitMQ 异步通信，互不影响

## 十、Identity 微服务

Identity 微服务是一个独立的认证授权服务，基于 ASP.NET Core Identity 和 JWT（JSON Web Token）实现用户注册、登录和身份验证功能。它为整个电商微服务架构提供统一的身份管理和安全保障。

### 1. 创建 Identity 微服务解决方案

Identity 微服务采用最小化 ASP.NET Core Web API 项目结构，集成 ASP.NET Core Identity 和 SQL Server 数据库。

**项目结构：**

```
src/Services/Identity/
└── Identity.API/
    ├── Controllers/
    │   └── AuthController.cs         # 认证控制器（注册/登录）
    ├── DTOs/
    │   ├── LoginDto.cs               # 登录请求 DTO
    │   └── RegisterDto.cs            # 注册请求 DTO
    ├── Data/
    │   └── ApplicationDbContext.cs   # EF Core 数据库上下文
    ├── Models/
    │   └── ApplicationUser.cs        # 自定义用户模型
    ├── Migrations/
    │   └── 20260624064851_InitialCreate.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Dockerfile
    ├── Identity.API.csproj
    ├── Identity.API.http
    ├── Program.cs
    ├── appsettings.Development.json
    └── appsettings.json
```

### 2. 创建 Identity Model 和 Context

**ApplicationUser 模型：**

```csharp
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
}
```

**ApplicationDbContext 上下文：**

```csharp
// Data/ApplicationDbContext.cs
using Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}
```

**NuGet 包依赖：**

```xml
<!-- Identity.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.9" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.9">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.9" />
    <PackageReference Include="Swashbuckle.AspNetCore.Swagger" Version="10.2.3" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="10.2.3" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.2.3" />
  </ItemGroup>

</Project>
```

| 包名 | 用途 |
|------|------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | ASP.NET Core Identity + EF Core |
| Microsoft.EntityFrameworkCore.SqlServer | SQL Server 数据库支持 |
| Microsoft.AspNetCore.Authentication.JwtBearer | JWT 认证中间件 |
| Swashbuckle.AspNetCore.Swagger* | Swagger/OpenAPI 文档支持 |

### 3. 配置 App Settings

**appsettings.json：**

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Server=localhost;Database=IdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "learn-dotnet-ecommerce-microservices",
    "Issuer": "learn-dotnet-ecommerce-microservices",
    "Audience": "learn-dotnet-ecommerce-microservices",
    "DurationInMinutes": 60
  }
}
```

**配置项说明：**

| 配置项 | 值 | 说明 |
|--------|-----|------|
| ConnectionStrings:IdentityConnection | SQL Server 连接字符串 | Identity 数据库连接 |
| Jwt:Key | 密钥字符串 | 用于签名 JWT 的对称密钥 |
| Jwt:Issuer | 发行者名称 | JWT 的 iss 声明值 |
| Jwt:Audience | 受众名称 | JWT 的 aud 声明值 |
| Jwt:DurationInMinutes | 60 | Token 有效期（分钟） |

### 4. 配置 Program.cs

```csharp
// Program.cs
using System.Text;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

**配置流程：**

```
AddDbContext → AddIdentity → AddAuthentication → AddAuthorization → AddControllers
                    ↓               ↓
              EF Core 配置     JWT Bearer 配置
```

### 5. 创建 DTOs

**RegisterDto（注册请求）：**

```csharp
// DTOs/RegisterDto.cs
namespace Identity.API.DTOs;

public record RegisterDto(
    string Name,
    string Email,
    string Password);
```

**LoginDto（登录请求）：**

```csharp
// DTOs/LoginDto.cs
namespace Identity.API.DTOs;

public record LoginDto(
    string Email,
    string Password);
```

### 6. 创建 Authentication Controller

```csharp
// Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.API.DTOs;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto register)
    {
        var user = new ApplicationUser
        {
            UserName = register.Email,
            Email = register.Email,
            Name = register.Name,
        };

        var result = await userManager.CreateAsync(user, register.Password);
        logger.LogInformation($"User {register.Email} registration attempted.");

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Registration successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var user = await userManager.FindByEmailAsync(login.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, login.Password))
            return Unauthorized();

        var token = generateToken(user);
        logger.LogInformation($"User {login.Email} logged in successfully.");
        return Ok(new { Token = token });
    }

    private string generateToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("uid", user.Id),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:DurationInMinutes"])),
            signingCredentials: credentials
        );

        logger.LogInformation($"JWT Token Generated for user {user.Email}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**API 端点：**

| 端点 | 方法 | 功能 | 参数 | 返回值 |
|------|------|------|------|--------|
| `/api/auth` | POST | 用户注册 | `RegisterDto` | `{ Message: "Registration successfully" }` |
| `/api/auth/login` | POST | 用户登录 | `LoginDto` | `{ Token: "JWT_TOKEN" }` |

**JWT Token 结构：**

```json
{
  "sub": "user@example.com",
  "name": "user@example.com",
  "uid": "550e8400-e29b-41d4-a716-446655440000",
  "iss": "learn-dotnet-ecommerce-microservices",
  "aud": "learn-dotnet-ecommerce-microservices",
  "exp": 1719175200
}
```

### 7. 修改 Launch Settings

**launchSettings.json：**

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5265",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7045;http://localhost:5265",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**端口配置：**

| 配置 | 端口 |
|------|------|
| HTTP | 5265 |
| HTTPS | 7045 |

### 8. Docker 配置

**Dockerfile：**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Identity/Identity.API/Identity.API.csproj", "src/Services/Identity/Identity.API/"]
RUN dotnet restore "src/Services/Identity/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Identity/Identity.API"
RUN dotnet build "./Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
```

**Docker Compose 配置：**

```yaml
# docker-compose.yml
services:
  identity.api:
    image: identity.api
    build:
      context: .
      dockerfile: src/Services/Identity/Identity.API/Dockerfile
    environment:
      - ConnectionStrings__IdentityConnection=Server=sqlserver;Database=IdentityDb;User Id=sa;Password=Password@123;TrustServerCertificate=True;
    depends_on:
      - sqlserver
    ports:
      - "5265:8080"
```

### 9. 应用数据库迁移

**创建迁移命令：**

```bash
cd src/Services/Identity/Identity.API
dotnet ef migrations add InitialCreate --project . --startup-project .
```

**应用迁移命令：**

```bash
cd src/Services/Identity/Identity.API
dotnet ef database update --project . --startup-project .
```

**生成的数据库表：**

| 表名 | 用途 |
|------|------|
| AspNetUsers | 用户信息 |
| AspNetRoles | 角色信息 |
| AspNetUserRoles | 用户-角色关联 |
| AspNetUserClaims | 用户声明 |
| AspNetUserLogins | 用户登录信息 |
| AspNetUserTokens | 用户令牌 |

### 10. JWT Demo

**完整认证流程演示：**

```
┌─────────────┐     ┌──────────────────┐     ┌──────────────┐
│   Client    │     │   Identity.API   │     │   SQL Server │
│ (Browser/   │     │   (AuthService)  │     │  (IdentityDb)│
│  API Client)│     │                  │     │              │
└──────┬──────┘     └────────┬─────────┘     └───────┬──────┘
       │                     │                       │
       │ 1. POST /api/auth   │                       │
       │    (RegisterDto)    │                       │
       │────────────────────>│                       │
       │                     │ 2. CreateAsync       │
       │                     │──────────────────────>│
       │                     │                       │
       │ 3. 200 OK           │                       │
       │    { Message }      │                       │
       │<────────────────────│                       │
       │                     │                       │
       │ 4. POST /api/auth/  │                       │
       │    login            │                       │
       │    (LoginDto)       │                       │
       │────────────────────>│                       │
       │                     │ 5. FindByEmailAsync  │
       │                     │──────────────────────>│
       │                     │                       │
       │                     │ 6. CheckPasswordAsync│
       │                     │──────────────────────>│
       │                     │                       │
       │                     │ 7. 生成 JWT Token    │
       │                     │                       │
       │ 8. 200 OK           │                       │
       │    { Token }        │                       │
       │<────────────────────│                       │
       │                     │                       │
       │ 9. 请求其他微服务    │                       │
       │    Authorization:   │                       │
       │    Bearer <token>   │                       │
       │────────────────────>│                       │
```

**验证步骤：**

1. **注册用户：**
   ```bash
   curl -X POST http://localhost:5265/api/auth \
     -H "Content-Type: application/json" \
     -d '{
       "name": "John Doe",
       "email": "john@example.com",
       "password": "Password@123"
     }'
   ```
   返回：`{ "message": "Registration successfully" }`

2. **登录获取 Token：**
   ```bash
   curl -X POST http://localhost:5265/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "john@example.com",
       "password": "Password@123"
     }'
   ```
   返回：`{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }`

3. **使用 Token 访问受保护资源：**
   ```bash
   curl -X GET http://localhost:5001/api/orders \
     -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   ```

**访问 Swagger 文档：**

```
http://localhost:5265/swagger/index.html
```

**JWT Token 解码示例：**

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
.
{
  "sub": "john@example.com",
  "name": "john@example.com",
  "uid": "550e8400-e29b-41d4-a716-446655440000",
  "iss": "learn-dotnet-ecommerce-microservices",
  "aud": "learn-dotnet-ecommerce-microservices",
  "nbf": 1719175200,
  "exp": 1719178800
}
```

**安全注意事项：**

| 风险 | 解决方案 |
|------|----------|
| 密钥泄露 | 使用环境变量存储密钥，生产环境使用更长的随机密钥 |
| Token 劫持 | 使用 HTTPS，设置合理的 Token 过期时间 |
| 密码泄露 | ASP.NET Core Identity 自动使用 BCrypt 哈希存储密码 |
| 无状态认证 | 使用 HttpOnly Cookie 或 Authorization Header 传递 Token |

## 十一、实现 ELK

### 1. 创建 Logging 项目

创建独立的日志基础设施项目 `Infrastructure.Logging`，用于集中管理 Serilog 配置和 Elasticsearch 集成。

**项目结构：**
```
src/Infrastructure/Infrastructure.Logging/
├── Infrastructure.Logging.csproj
└── Logging.cs
```

### 2. 添加所需的 NuGet 包

在 `Infrastructure.Logging` 项目中添加以下依赖：

| 包名 | 版本 | 说明 |
|------|------|------|
| Elastic.Serilog.Sinks | 9.0.0 | Serilog 的 Elasticsearch 输出插件 |
| Serilog.AspNetCore | 10.0.0 | Serilog 的 ASP.NET Core 集成 |
| Serilog.Enrichers.Environment | 3.0.1 | 添加环境变量信息到日志 |
| Serilog.Exceptions | 8.4.0 | 异常详情 enricher |
| Serilog.Sinks.Console | 6.1.1 | 控制台日志输出 |
| Microsoft.Extensions.Configuration | 10.0.9 | 配置绑定 |

**csproj 文件内容：**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Elastic.Serilog.Sinks" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.9" />
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
    <PackageReference Include="Serilog.Exceptions" Version="8.4.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
  </ItemGroup>
</Project>
```

### 3. 添加项目引用

在所有需要启用日志的微服务项目中添加对 `Infrastructure.Logging` 的项目引用：

- Catalog.API
- Basket.API
- Discount.API
- Ordering.API
- Payment.API
- Identity.API

### 4. 创建日志扩展方法

在 `Infrastructure.Logging/Logging.cs` 中创建静态配置类，提供统一的日志配置：

```csharp
namespace Infrastructure.Logging;

public static class Logging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger => 
        (context, loggerConfiguration) =>
    {
        var env = context.HostingEnvironment;
        var configuration = context.Configuration;

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", env.ApplicationName)
            .Enrich.WithProperty("Environment", env.EnvironmentName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        if (env.IsDevelopment())
        {
            loggerConfiguration.MinimumLevel.Debug();
        }

        var elasticUri = configuration.GetValue<string>("ElasticConfiguration:Uri");

        if (!string.IsNullOrWhiteSpace(elasticUri))
        {
            var dataStream = new DataStreamName(
                "logs",
                env.ApplicationName?.ToLower().Replace('.', '-') ?? "unknown",
                env.EnvironmentName?.ToLower() ?? "development"
            );

            loggerConfiguration.WriteTo.Elasticsearch(
                new[] { new Uri(elasticUri) },
                opts =>
                {
                    opts.DataStream = dataStream;
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                },
                _ => { }
            );
        }
    };
}
```

**配置说明：**

| 配置项 | 说明 |
|--------|------|
| MinimumLevel | 默认信息级别，开发环境降级为 Debug |
| System/Microsoft 日志 | 仅显示警告及以上级别 |
| Enrich.FromLogContext | 从日志上下文补充信息 |
| Enrich.WithProperty | 添加应用名称和环境名称 |
| Enrich.WithExceptionDetails | 记录异常详细信息 |
| WriteTo.Console | 输出到控制台 |
| WriteTo.Elasticsearch | 输出到 Elasticsearch（如果配置了 URI） |

### 5. 在 Catalog Service 中引入日志

在 `Catalog.API/Program.cs` 中添加日志配置：

```csharp
using Infrastructure.Logging;
using Serilog;

// ... 其他配置 ...

builder.Host.UseSerilog(Logging.ConfigureLogger);

var app = builder.Build();
// ... 其余代码 ...
```

同时在 `appsettings.json` 中添加 Elasticsearch 配置（可选，用于本地开发）：

```json
{
  "ElasticConfiguration": {
    "Uri": "http://localhost:9200"
  }
}
```

### 6. 在其他微服务中进行相同的日志配置

对所有微服务项目执行相同操作：

**6.1 添加项目引用：**
```xml
<ProjectReference Include="..\..\Infrastructure\Infrastructure.Logging\Infrastructure.Logging.csproj" />
```

**6.2 Program.cs 中添加 Serilog：**
```csharp
using Infrastructure.Logging;
using Serilog;

builder.Host.UseSerilog(Logging.ConfigureLogger);
```

**6.3 涉及的服务列表：**

| 服务 | 项目路径 |
|------|----------|
| Catalog.API | src/Services/Catalog/Catalog.API/ |
| Basket.API | src/Services/Basket/Basket.API/ |
| Discount.API | src/Services/Discount/Discount.API/ |
| Ordering.API | src/Services/Ordering/Ordering.API/ |
| Payment.API | src/Services/Payment/Payment.API/ |
| Identity.API | src/Services/Identity/Identity.API/ |

### 7. 更新 Docker 配置

**7.1 为各微服务创建 Dockerfile：**

以 Catalog.API 为例：
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Catalog/Catalog.API/Catalog.API.csproj", "src/Services/Catalog/Catalog.API/"]
COPY ["src/Services/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Services/Catalog/Catalog.Application/"]
COPY ["src/Services/Catalog/Catalog.Core/Catalog.Core.csproj", "src/Services/Catalog/Catalog.Core/"]
COPY ["src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Services/Catalog/Catalog.Infrastructure/"]
COPY ["src/Infrastructure/EventBus.Messages/EventBus.Messages.csproj", "src/Infrastructure/EventBus.Messages/"]
COPY ["src/Infrastructure/Infrastructure.Logging/Infrastructure.Logging.csproj", "src/Infrastructure/Infrastructure.Logging/"]
RUN dotnet restore "src/Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Catalog/Catalog.API"
RUN dotnet build "./Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
```

**7.2 更新 docker-compose.yaml，添加 Elasticsearch 和 Kibana 服务：**

```yaml
services:
  # ... 现有服务 ...

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.3.6
  
  kibana:
    image: docker.elastic.co/kibana/kibana:9.3.6

volumes:
  # ... 现有卷 ...
  elasticsearch_data:
```

**7.3 更新 docker-compose.override.yaml，配置环境变量：**

为每个微服务添加 Elasticsearch 连接配置：
```yaml
environment:
  - ElasticConfiguration__Uri=http://elasticsearch:9200
```

添加 Elasticsearch 和 Kibana 的详细配置：
```yaml
elasticsearch:
  container_name: elasticsearch
  environment:
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    - discovery.type=single-node
    - xpack.security.enabled=false
  ports:
    - "9200:9200"
  volumes:
    - elasticsearch_data:/usr/share/elasticsearch/data

kibana:
  container_name: kibana
  environment:
    - ELASTICSEARCH_URL=http://elasticsearch:9200
  depends_on:
    - elasticsearch
  ports:
    - "5601:5601"
```

### 8. 启动与验证

**8.1 启动所有服务：**
```bash
docker-compose up -d
```

**8.2 验证 Elasticsearch 是否运行：**
```bash
curl http://localhost:9200
```

**8.3 访问 Kibana：**
```
http://localhost:5601
```

**8.4 在 Kibana 中查看日志：**

1. 进入 **Stack Management** -> **Index Patterns**
2. 创建 Index Pattern：`logs-catalog-api-development`
3. 进入 **Discover** 页面查看实时日志
4. 可以按应用名称筛选：`ApplicationName: catalog.api`

### 9. 日志数据流

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│  Microservice │──→│  Serilog     │──→│  Elasticsearch   │
│  (Catalog)   │     │  (Logging)   │     │  (Port 9200)     │
└─────────────┘     └──────────────┘     └─────────────────┘
                            │                      │
                            ▼                      ▼
                     Console Output          Kibana UI
                                              (Port 5601)
```

### 10. 日志 enricher 说明

| Enricher | 作用 | 示例值 |
|----------|------|--------|
| FromLogContext | 从日志上下文添加额外属性 | 请求 ID、用户 ID |
| WithProperty(ApplicationName) | 添加应用名称 | "catalog.api" |
| WithProperty(Environment) | 添加环境名称 | "development" |
| WithExceptionDetails | 格式化异常详情 | 堆栈跟踪、内联展示 |

### 11. 优势总结

| 特性 | 说明 |
|------|------|
| 集中式日志 | 所有微服务的日志统一存储到 Elasticsearch |
| 结构化查询 | 支持复杂的查询和过滤条件 |
| 可视化分析 | 通过 Kibana 进行日志分析和可视化 |
| 异常详情 | 自动内联展示异常信息，便于排查问题 |
| 环境隔离 | 通过 DataStream 区分不同环境和应用 |
| 可扩展性 | 可以轻松添加更多微服务和日志源 |

## 十二、实现 API 网关

### 1. 创建 API Gateway 项目

创建独立的 API 网关服务 `ApiGateway`，作为所有微服务的统一入口，负责路由、认证和跨域等横切关注点。

**项目结构：**
```
src/ApiGateway/ApiGateway/
├── ApiGateway.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── ocelot.Development.json
├── Middleware/
│   └── CorrelationIdMiddleware.cs
├── Dockerfile
└── Properties/launchSettings.json
```

### 2. 安装所需的 NuGet 包

| 包名 | 版本 | 说明 |
|------|------|------|
| Ocelot | 24.1.0 | API 网关路由中间件 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.9 | JWT Bearer 认证 |
| Microsoft.AspNetCore.Cors | 2.3.11 | 跨域资源共享 |
| Microsoft.AspNetCore.OpenApi | 10.0.0 | OpenAPI 支持 |
| Serilog | 4.3.1 | 结构化日志 |

**csproj 文件内容：**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.Cors" Version="2.3.11" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    <PackageReference Include="Ocelot" Version="24.1.0" />
    <PackageReference Include="Serilog" Version="4.3.1" />
  </ItemGroup>
</Project>
```

### 3. 添加 Ocelot 配置文件

在 `ocelot.Development.json` 中定义路由规则，将上游请求路由到下游微服务：

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/Catalog/GetAllProducts",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "host.docker.internal", "Port": 8000 }],
      "UpstreamPathTemplate": "/Catalog/GetAllProducts",
      "UpstreamHttpMethod": ["GET"],
      "AddHeadersToRequest": { "x-correlation-id": "{X-Correlation-Id}" }
    },
    {
      "DownstreamPathTemplate": "/api/v1/Catalog/{id}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "host.docker.internal", "Port": 8000 }],
      "UpstreamPathTemplate": "/Catalog/{id}",
      "UpstreamHttpMethod": ["GET", "DELETE"],
      "AddHeadersToRequest": { "x-correlation-id": "{X-Correlation-Id}" }
    },
    {
      "DownstreamPathTemplate": "/api/v1/Basket/Checkout",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "host.docker.internal", "Port": 8000 }],
      "UpstreamPathTemplate": "/Basket/Checkout",
      "UpstreamHttpMethod": ["POST"],
      "AddHeadersToRequest": { "x-correlation-id": "{X-Correlation-Id}" },
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "DownstreamPathTemplate": "/api/v1/Order",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "host.docker.internal", "Port": 8000 }],
      "UpstreamPathTemplate": "/Order",
      "UpstreamHttpMethod": ["POST", "PUT"],
      "AddHeadersToRequest": { "x-correlation-id": "{X-Correlation-Id}" }
    },
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "host.docker.internal", "Port": 8000 }],
      "UpstreamPathTemplate": "/identity/api/auth/{everything}",
      "UpstreamHttpMethod": ["GET", "POST"],
      "AddHeadersToRequest": { "x-correlation-id": "{X-Correlation-Id}" }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:8010"
  }
}
```

**路由配置说明：**

| 配置项 | 说明 |
|--------|------|
| UpstreamPathTemplate | 客户端请求的路径模板 |
| DownstreamPathTemplate | 下游微服务的实际路径 |
| DownstreamHostAndPorts | 下游服务的地址和端口 |
| UpstreamHttpMethod | 允许的 HTTP 方法 |
| AddHeadersToRequest | 自动添加的请求头（如关联 ID） |
| AuthenticationOptions | 需要认证的选项 |

### 4. 添加应用配置

在 `appsettings.json` 中配置 JWT 和 CORS：

```json
{
  "Jwt": {
    "Key": "learn-dotnet-ecommerce-microservices",
    "Issuer": "learn-dotnet-ecommerce-microservices",
    "Audience": "learn-dotnet-ecommerce-microservices",
    "DurationInMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

### 5. 配置 Program.cs

在 `Program.cs` 中整合 Ocelot、JWT 认证和中间件：

```csharp
using System.Text;
using ApiGateway.Middleware;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 加载 Ocelot 配置
var env = builder.Environment.EnvironmentName;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"ocelot.{env}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings.GetSection("Key").Value;
var issuer = jwtSettings.GetSection("Issuer").Value;
var audience = jwtSettings.GetSection("Audience").Value;

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 配置 JWT 认证
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseCors("AllowedOrigins");
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");
await app.UseOcelot();
app.Run();
```

### 6. 添加关联 ID 中间件

创建 `Middleware/CorrelationIdMiddleware.cs`，用于在请求链路中传递关联 ID，便于日志追踪：

```csharp
using Serilog.Context;

namespace ApiGateway.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationIdHeader = "x-correlation-id";

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Request.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.LogInformation("Correlation Id set: {CorrelationId}", correlationId);
            await next(context);
        }
    }
}
```

**作用：**

| 功能 | 说明 |
|------|------|
| 请求追踪 | 为每个请求生成或提取唯一关联 ID |
| 日志关联 | 通过 Serilog.Context 将关联 ID 附加到所有日志 |
| 跨服务追踪 | 通过 Ocelot 的 AddHeadersToRequest 传递到下���服务 |

### 7. 配置 Docker Compose

**7.1 API Gateway Dockerfile：**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ApiGateway/ApiGateway/ApiGateway.csproj", "src/ApiGateway/ApiGateway/"]
RUN dotnet restore "src/ApiGateway/ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

**7.2 更新 docker-compose.yaml：**

```yaml
services:
  ocelot.apigateway:
    image: ocelot.apigateway
    build:
      context: .
      dockerfile: src/ApiGateway/ApiGateway/Dockerfile
```

**7.3 更新 docker-compose.override.yaml：**

```yaml
services:
  ocelot.apigateway:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Key=super_secure_secret_key1234567890@#*&!@%_
      - Jwt__Issuer=learn-dotnet-ecommerce-microservices
      - Jwt__Audience=learn-dotnet-ecommerce-microservices
      - Jwt_DurationInMinutes=60
    depends_on:
      - identity.api
      - catalog.api
      - basket.api
      - ordering.api
      - payment.api
      - discount.api
    ports:
      - "8010:8080"
```

### 8. 启动与验证

**8.1 启动所有服务：**
```bash
docker-compose up -d
```

**8.2 通过网关访问微服务：**

| 接口 | 网关路径 | 说明 |
|------|----------|------|
| 产品列表 | `GET http://localhost:8010/Catalog/GetAllProducts` | 路由到 Catalog API |
| 单个产品 | `GET http://localhost:8010/Catalog/{id}` | 路由到 Catalog API |
| 购物车 | `GET http://localhost:8010/Basket/{userName}` | 路由到 Basket API |
| 结算 | `POST http://localhost:8010/Basket/Checkout` | 路由到 Basket API（需认证） |
| 订单 | `GET http://localhost:8010/Order/{userName}` | 路由到 Ordering API |
| 创建订单 | `POST http://localhost:8010/Order` | 路由到 Ordering API |
| 认证 | `POST http://localhost:8010/identity/api/auth/register` | 路由到 Identity API |

**8.3 请求链路示例：**

```
客户端请求
    ↓
http://localhost:8010/Catalog/GetAllProducts
    ↓
API Gateway (Ocelot)
    ├─ 添加 x-correlation-id 请求头
    ├─ 验证 JWT Token（如需认证）
    └─ 路由到下游服务
    ↓
http://host.docker.internal:8000/api/v1/Catalog/GetAllProducts
    ↓
Catalog API 处理并返回响应
```

### 9. 网关架构

```
┌──────────┐       ┌──────────────────┐       ┌──────────────┐
│  Client   │──────▶│  API Gateway     │──────▶│  Catalog API │
│           │       │  (Port 8010)     │       │  (Port 8000) │
└──────────┘       │                  │       └──────────────┘
                   │  - 路由分发      │
┌──────────┐       │  - JWT 认证      │       ┌──────────────┐
│  Client   │──────▶│  - CORS          │──────▶│  Basket API  │
│           │       │  - 关联 ID       │       │  (Port 8000) │
└──────────┘       │  - 请求转换      │       └──────────────┘
                   └──────────────────┘
                           │
                   ┌───────┴──────────────┐
                   ▼                      ▼
            ┌──────────────┐      ┌──────────────┐
            │ Ordering API │      │ Identity API │
            └──────────────┘      └──────────────┘
```

### 10. 优势总结

| 特性 | 说明 |
|------|------|
| 统一入口 | 所有客户端请求通过单一端口（8010）访问 |
| 路由转发 | Ocelot 自动将请求路由到对应微服务 |
| JWT 验证 | 网关层统一验证 Token，减轻微服务负担 |
| 跨域支持 | 集中配置 CORS，避免在每个服务中重复配置 |
| 关联追踪 | 通过 CorrelationIdMiddleware 实现请求链路追踪 |
| 请求转换 | 自动转换路径格式（如 /Catalog → /api/v1/Catalog） |
| 动态配置 | Ocelot 支持热重载配置，无需重启服务 |

## 十三、实现 Aspire

### 1. 创建 Aspire 项目

安装 Aspire 模板并创建 AppHost 和 ServiceDefaults 项目：

```bash
dotnet new install Aspire.ProjectTemplates
dotnet new aspire-apphost -n Aspire.AppHost
dotnet new aspire-servicedefaults -n Aspire.ServiceDefaults
```

**创建的项目说明：**

| 项目 | 用途 | 端口 |
|------|------|------|
| Aspire.AppHost | 编排中心，管理所有微服务和基础设施资源 | 19888 (Dashboard) |
| Aspire.ServiceDefaults | 共享项目，为微服务提供可观测性配置 | 无独立端口 |

### 2. 安装项目引用

将所有微服务项目添加到 Aspire.AppHost 中：

```bash
dotnet add Aspire.AppHost reference ApiGateway
dotnet add Aspire.AppHost reference Basket.API
dotnet add Aspire.AppHost reference Catalog.API
dotnet add Aspire.AppHost reference Discount.API
dotnet add Aspire.AppHost reference Identity.API
dotnet add Aspire.AppHost reference Ordering.API
dotnet add Aspire.AppHost reference Payment.API
```

**项目引用关系：**

| 被引用的项目 | 在 Aspire 中的角色 | 说明 |
|-------------|-------------------|------|
| ApiGateway | 上游服务 | 统一入口，路由到所有微服务 |
| Catalog.API | 微服务 | 产品目录服务 |
| Basket.API | 微服务 | 购物车服务 |
| Ordering.API | 微服务 | 订单服务 |
| Discount.API | 微服务 | 折扣服务 |
| Identity.API | 微服务 | 身份认证服务 |
| Payment.API | 微服务 | 支付服务 |

### 3. 安装基础设施依赖包

安装 Aspire 托管的各种基础设施包：

```bash
dotnet add package Aspire.Hosting.MongoDB
dotnet add package Aspire.Hosting.Redis
dotnet add package Aspire.Hosting.PostgreSQL
dotnet add package Aspire.Hosting.SqlServer
dotnet add package Aspire.Hosting.RabbitMQ
```

**基础设施包对照表：**

| NuGet 包 | 基础设施 | 被哪个微服务使用 | 端口 |
|----------|----------|-----------------|------|
| Aspire.Hosting.MongoDB | MongoDB | Catalog.API | 27017 |
| Aspire.Hosting.Redis | Redis | Basket.API | 6379 |
| Aspire.Hosting.PostgreSQL | PostgreSQL | Discount.API | 5432 |
| Aspire.Hosting.SqlServer | SQL Server | Ordering.API, Identity.API | 1433 |
| Aspire.Hosting.RabbitMQ | RabbitMQ | Basket, Ordering, Payment | 5672 / 15672 |
```

### 4. 定义 App Host - 第一部分

创建 `Aspire.AppHost` 项目，作为 Orchestration 中心管理所有微服务和基础设施。

**项目结构：**
```
src/Aspire/
├── Aspire.AppHost/
│   ├── Aspire.AppHost.csproj
│   ├── AppHost.cs
│   ├── aspire.config.json
│   ├── appsettings.json
│   └── appsettings.Development.json
└── Aspire.ServiceDefaults/
    ├── Aspire.ServiceDefaults.csproj
    └── Extensions.cs
```

**AppHost.csproj：**
```xml
<Project Sdk="Aspire.AppHost.Sdk/13.4.6">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>b56c3360-eb25-4940-aadb-5d54e48c6ab2</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.MongoDB" Version="13.4.6" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="13.4.6" />
    <PackageReference Include="Aspire.Hosting.RabbitMQ" Version="13.4.6" />
    <PackageReference Include="Aspire.Hosting.Redis" Version="13.4.6" />
    <PackageReference Include="Aspire.Hosting.SqlServer" Version="13.4.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\ApiGateway\ApiGateway\ApiGateway.csproj" />
    <ProjectReference Include="..\..\Services\Basket\Basket.API\Basket.API.csproj" />
    <ProjectReference Include="..\..\Services\Catalog\Catalog.API\Catalog.API.csproj" />
    <ProjectReference Include="..\..\Services\Discount\Discount.API\Discount.API.csproj" />
    <ProjectReference Include="..\..\Services\Identity\Identity.API\Identity.API.csproj" />
    <ProjectReference Include="..\..\Services\Ordering\Ordering.API\Ordering.API.csproj" />
    <ProjectReference Include="..\..\Services\Payment\Payment.API\Payment.API.csproj" />
  </ItemGroup>

</Project>
```

### 5. 添加所需的项目引用

将所有微服务项目添加到 AppHost 中：

| 项目引用 | 说明 |
|----------|------|
| ApiGateway | API 网关 |
| Basket.API | 购物车服务 |
| Catalog.API | 产品目录服务 |
| Discount.API | 折扣服务 |
| Identity.API | 身份认证服务 |
| Ordering.API | 订单服务 |
| Payment.API | 支付服务 |

### 6. 扩展 App Host 功能

**6.1 定义基础设施资源：**

```csharp
// MongoDB - Catalog
var mongo = builder.AddMongoDB("catalogdb").WithDataVolume();

// Redis - Basket
var redis = builder.AddRedis("basketdb");

// Postgres - Discount
var postgres = builder.AddPostgres("discountdb").WithDataVolume();
var discountDb = postgres.AddDatabase("discount-db");

// Sqlserver - Ordering + Identity
var sqlserver = builder.AddSqlServer("sqlserver").WithDataVolume();
var orderingDb = sqlserver.AddDatabase("OrderingDb");
var identityDb = sqlserver.AddDatabase("IdentityDb");

// RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbitmq");
```

**6.2 注册微服务并连接基础设施：**

```csharp
// APIs
var catalog = builder.AddProject<Projects.Catalog_API>("catalog")
    .WithReference(mongo);

var basket = builder.AddProject<Projects.Basket_API>("basket")
    .WithReference(redis)
    .WithReference(rabbitmq);

var ordering = builder.AddProject<Projects.Ordering_API>("ordering")
    .WithReference(orderingDb)
    .WithReference(rabbitmq);

var discount = builder.AddProject<Projects.Discount_API>("discount")
    .WithReference(discountDb);

var payment = builder.AddProject<Projects.Payment_API>("payment")
    .WithReference(rabbitmq);

var identity = builder.AddProject<Projects.Identity_API>("identity")
    .WithReference(identityDb)
    .WithEnvironment("Jwt__Key","super_secure_secret_key1234567890#@!")
    .WithEnvironment("Jwt__Issuer","learn-dotnet-ecommerce-microservices")
    .WithEnvironment("Jwt__Audience","learn-dotnet-ecommerce-microservices")
    .WithEnvironment("Jwt__DurationInMinutes","60");
```

**5.3 配置 API Gateway 路由：**

```csharp
// API Gateway
var gateway = builder.AddProject<Projects.ApiGateway>("apigateway");
gateway
    .WithReference(catalog)
    .WithReference(basket)
    .WithReference(ordering)
    .WithReference(discount)
    .WithReference(payment);

builder.Build().Run();
```

**WithReference 说明：**

| 服务 | 引用资源 | 连接字符串注入方式 |
|------|----------|-------------------|
| Catalog | MongoDB | `DatabaseSettings__ConnectionString` |
| Basket | Redis + RabbitMQ | `CacheSettings__ConnectionString` + `EventBusSettings__HostAddress` |
| Ordering | SQL Server + RabbitMQ | `DatabaseSettings__ConnectionString` + `EventBusSettings__HostAddress` |
| Discount | PostgreSQL | `DatabaseSettings__ConnectionString` |
| Payment | RabbitMQ | `EventBusSettings__HostAddress` |
| Identity | SQL Server | `DatabaseSettings__ConnectionString` |

### 7. 创建 ServiceDefaults 项目

`Aspire.ServiceDefaults` 是一个共享项目，为每个微服务提供通用的可观测性配置：

**Aspire.ServiceDefaults.csproj：**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAspireSharedProject>true</IsAspireSharedProject>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />

    <PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="10.6.0" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="10.6.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.15.3" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.15.3" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.15.2" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.15.1" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.15.1" />
  </ItemGroup>
</Project>
```

**核心功能模块：**

| 功能 | 方法 | 说明 |
|------|------|------|
| 开放遥测 | `ConfigureOpenTelemetry()` | 配置 Metrics 和 Tracing |
| 健康检查 | `AddDefaultHealthChecks()` | 添加自检查询 |
| 服务发现 | `AddServiceDiscovery()` | 支持动态服务发现 |
| HTTP 韧性 | `AddStandardResilienceHandler()` | 自动重试和熔断 |
| 端点映射 | `MapDefaultEndpoints()` | 暴露健康检查和存活端点 |

**Metrics 采集：**
- ASP.NET Core 请求指标
- HTTP 客户端调用指标
- 运行时资源指标（CPU、内存等）

**Tracing 配置：**
- 排除健康检查端点的追踪
- HTTP 客户端追踪
- 应用名称作为 Trace Source

### 8. 解决 Aspire 异常问题

**7.1 常见问题：连接字符串格式不匹配**

Aspire 自动生成的连接字符串可能与微服务期望的格式不同。例如：

| 来源 | 格式 |
|------|------|
| Aspire 自动生成 | `tcp:sqlserver:1433`（服务发现格式） |
| 微服务期望 | `Server=sqlserver;Port=1433;Database=OrderingDb;...` |

**8.2 解决方案：自定义连接字符串**

在 `AppHost.cs` 中通过 `.WithEnvironment()` 覆盖连接字符串：

```csharp
var identity = builder.AddProject<Projects.Identity_API>("identity")
    .WithReference(identityDb)
    .WithEnvironment("DatabaseSettings__ConnectionString", 
        "Server=identity.db;Port=1434;Database=IdentityDb;User Id=sa;Password=Password@123");
```

### 9. 连接字符串问题详解

**9.1 问题描述：**

Aspire 默认使用服务发现机制，生成的连接字符串格式为：
```
tcp:<service-name>:<port>
```

但微服务的 `appsettings.json` 中期望的是传统格式：
```
Server=<host>;Port=<port>;Database=<name>;User Id=<user>;Password=<pwd>
```

**9.2 影响范围：**

| 服务 | 数据库 | 问题表现 |
|------|--------|----------|
| Catalog.API | MongoDB | MongoDB 驱动可能无法解析服务发现格式 |
| Ordering.API | SQL Server | EF Core 无法识别 `tcp:` 协议 |
| Identity.API | SQL Server | 同上 |
| Discount.API | PostgreSQL | Npgsql 连接失败 |

### 10. 修复连接字符串问题

**方案一：在 AppHost 中显式设置连接字符串**

```csharp
var catalog = builder.AddProject<Projects.Catalog_API>("catalog")
    .WithReference(mongo)
    .WithEnvironment("DatabaseSettings__ConnectionString", 
        "mongodb://localhost:27017");
```

**方案二：使用 Aspire 的资源命名约定**

确保微服务中的连接字符串键名与 Aspire 注入的键名一致：

| 微服务 | 配置键 | Aspire 注入值 |
|--------|--------|---------------|
| Catalog | `DatabaseSettings__ConnectionString` | MongoDB 连接字符串 |
| Basket | `CacheSettings__ConnectionString` | Redis 连接字符串 |
| Ordering | `DatabaseSettings__ConnectionString` | SQL Server 连接字符串 |
| Discount | `DatabaseSettings__ConnectionString` | PostgreSQL 连接字符串 |
| Identity | `DatabaseSettings__ConnectionString` | SQL Server 连接字符串 |

**方案三：更新微服务的 appsettings.json**

确保连接字符串占位符与 Aspire 的环境变量注入格式匹配：

```json
{
  "DatabaseSettings": {
    "ConnectionString": "Server=localhost;Port=1433;Database=OrderingDb;User Id=sa;Password=Password@123"
  }
}
```

Aspire 会将 `DatabaseSettings__ConnectionString` 环境变量覆盖此值。

### 11. 启动与验证 Aspire

**10.1 启动 Aspire Dashboard：**
```bash
cd src/Aspire/Aspire.AppHost
dotnet run
```

**11.2 访问 Aspire Dashboard：**
```
http://localhost:19888
```

**10.3 Dashboard 功能：**

| 功能 | 说明 |
|------|------|
| 资源视图 | 查看所有微服务和基础设施的状态 |
| 分布式追踪 | 查看请求在各服务间的流转 |
| 指标监控 | 实时查看 Metrics 数据 |
| 日志聚合 | 集中查看各服务的日志 |
| 健康检查 | 监控各服务的健康状态 |

**11.4 架构概览：**

```
┌─────────────────────────────────────────────────┐
│              Aspire Dashboard                    │
│              (Port 19888)                        │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │ Catalog  │  │ Basket   │  │ Ordering │      │
│  │  API     │  │  API     │  │  API     │      │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘      │
│       │             │             │             │
│  ┌────┴─────────────┴─────────────┴─────┐      │
│  │         Service Discovery             │      │
│  └──────────────────┬────────────────────┘      │
│                     │                           │
│  ┌──────────────────┴────────────────────┐      │
│  │         Resilience & Telemetry        │      │
│  └──────────────────┬────────────────────┘      │
└─────────────────────┬───────────────────────────┘
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
   ┌────────┐   ┌────────┐   ┌────────┐
   │MongoDB │   │ Redis  │   │SQL Server│
   └────────┘   └────────┘   └────────┘
```

### 12. Aspire 优势总结

| 特性 | 说明 |
|------|------|
| 本地开发体验 | 一条命令启动所有服务和基础设施 |
| 服务发现 | 自动解析服务地址，无需硬编码 |
| 可观测性 | 内置 OpenTelemetry 集成，自动收集 Traces/Metrics/Logs |
| 韧性处理 | 自动添加重试、熔断等 HTTP 韧性策略 |
| 资源编排 | 声明式定义基础设施依赖关系 |
| Dashboard | 可视化监控所有服务和资源 |
| 环境变量注入 | 自动将连接字符串注入到微服务 |
| 开发效率 | 减少本地开发环境的配置复杂度 |

---

## 十四、系统架构与接口文档

### 1. 系统架构图

#### 1.1 整体架构

```
                            ┌────────────────────────────┐
                            │       客户端 / 前端        │
                            │   (Angular 21 / Postman)   │
                            └─────────────┬──────────────┘
                                          │ HTTP (REST)
                                          ▼
                            ┌────────────────────────────┐
                            │   API Gateway (Ocelot)    │
                            │       Port: 8010           │
                            │  - 路由转发                │
                            │  - JWT 鉴权                │
                            │  - 请求聚合                │
                            └─────────────┬──────────────┘
                                          │
        ┌─────────────────┬───────────────┼────────────────┬─────────────────┐
        ▼                 ▼               ▼                ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Catalog.API  │  │ Basket.API  │  │ Ordering.API │  │ Payment.API  │  │ Identity.API │
│   Port 8000  │  │   Port 8020 │  │  Port 8040   │  │  Port 8050   │  │  Port 8060   │
│              │  │              │  │              │  │              │  │              │
│ MongoDB      │  │ Redis        │  │ SQL Server   │  │ (无状态)     │  │ SQL Server   │
│ Products/    │  │ ShoppingCarts│  │ Orders/      │  │ 消费事件     │  │ Users        │
│ Brands/Types │  │              │  │ OutboxMsg    │  │              │  │              │
└──────────────┘  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────────────┘
                         │ gRPC            │                 │
                         ▼                 │                 │
                  ┌──────────────┐         │                 │
                  │ Discount.API│         │                 │
                  │  Port 8030  │         │                 │
                  │ PostgreSQL  │         │                 │
                  │  Coupons    │         │                 │
                  └──────────────┘         │                 │
                                           │                 │
                                           ▼                 ▼
                                   ┌────────────────────────────┐
                                   │   RabbitMQ (Event Bus)     │
                                   │   Port: 5672 / 15672       │
                                   │                            │
                                   │  Queues:                   │
                                   │  - basket-checkout-queue   │
                                   │  - order-created-queue     │
                                   └────────────────────────────┘
                                           ▲
                                   ┌────────────────────────────┐
                                   │     ELK 日志栈            │
                                   │  Elasticsearch: 9200       │
                                   │  Kibana: 5601             │
                                   │  Serilog 集中日志         │
                                   └────────────────────────────┘
```

#### 1.2 服务职责一览

| 服务 | 端口 | 数据库 | 职责 | 通信方式 |
|------|------|--------|------|----------|
| ocelot.apigateway | 8010 | - | 统一入口、路由、鉴权 | HTTP |
| catalog.api | 8000 | MongoDB | 商品/品牌/类型管理 | HTTP |
| basket.api | 8020 | Redis | 购物车管理 | HTTP + gRPC |
| discount.api | 8030 | PostgreSQL | 折扣券管理（gRPC） | gRPC |
| ordering.api | 8040 | SQL Server | 订单管理 + Outbox | HTTP + RabbitMQ |
| payment.api | 8050 | - | 支付处理（事件消费） | RabbitMQ |
| identity.api | 8060 | SQL Server | 用户注册登录、JWT 颁发 | HTTP |

#### 1.3 数据流向

```
[客户端] ──HTTP──> [Ocelot Gateway] ──路由──> [各微服务]
                                                  │
                                                  ├──[同步]──> [数据库]
                                                  │
                                                  ├──[同步 gRPC]──> [Basket -> Discount]
                                                  │
                                                  └──[异步事件]──> [RabbitMQ] ──> [其他服务]
                                                                                       │
                                                                                       └──[Serilog]──> [Elasticsearch]
```

---

### 2. 业务流程图

#### 2.1 完整电商购物流程

```
┌──────────┐    1. 注册/登录      ┌──────────────┐    颁发 JWT     ┌──────────┐
│          │ ───────────────────> │ Identity.API │ <─────────────  │          │
│          │ <─────────────────── │              │                 │          │
│          │     2. 携带 JWT      └──────────────┘                 │          │
│          │ ─────────────────────────────────────────────────────> │          │
│          │    3. 浏览商品         ┌──────────────┐                 │          │
│          │ ───────────────────>  │ Catalog.API  │ <── MongoDB     │          │
│          │ <───────────────────  │              │                 │          │
│          │    4. 加入购物车       ┌──────────────┐                 │          │
│          │ ───────────────────> │  Basket.API  │ <── Redis        │   客户端  │
│          │                       │      │       │                 │          │
│          │                       │      │ gRPC  │                 │          │
│          │                       │      ▼       │                 │          │
│          │                       │ Discount.API │ <── PostgreSQL  │          │
│          │                       │ (获取折扣)   │                 │          │
│          │ <───────────────────  │              │                 │          │
│  用户   │    5. 结账 (Checkout)                                  │          │
│          │ ──Bearer JWT────────> │  Basket.API  │                 │          │
│          │                       └──────┬───────┘                 │          │
│          │                              │                         │          │
│          │                              │ 发布事件                 │          │
│          │                              ▼                         │          │
│          │                       ┌──────────────┐                 │          │
│          │                       │   RabbitMQ   │                 │          │
│          │                       │ BasketCheckoutEvent            │          │
│          │                       └──────┬───────┘                 │          │
│          │                              │                         │          │
│          │                              ▼ 消费事件                │          │
│          │                       ┌──────────────┐                 │          │
│          │                       │ Ordering.API │ <── SQL Server  │          │
│          │                       │ (创建订单)   │   (写入Outbox)  │          │
│          │                       └──────┬───────┘                 │          │
│          │                              │                         │          │
│          │                              │ 发布 OrderCreatedEvent  │          │
│          │                              ▼                         │          │
│          │                       ┌──────────────┐                 │          │
│          │                       │   RabbitMQ   │                 │          │
│          │                       │ order-created-queue           │          │
│          │                       └──────┬───────┘                 │          │
│          │                              │                         │          │
│          │                              ▼ 消费事件                │          │
│          │                       ┌──────────────┐                 │          │
│          │                       │ Payment.API  │                 │          │
│          │                       │ (处理支付)   │                 │          │
│          │                       └──────────────┘                 │          │
└──────────┘                                                       └──────────┘
```

#### 2.2 事件驱动流转（Saga 协调）

```
[Basket.API]                 [RabbitMQ]                 [Ordering.API]              [RabbitMQ]              [Payment.API]
     │                            │                            │                          │                          │
     │ 1. POST /Basket/Checkout  │                            │                          │                          │
     │ (Bearer JWT)               │                            │                          │                          │
     ├───────────────────────────>│                            │                          │                          │
     │                            │                            │                          │                          │
     │ 2. 发布 BasketCheckoutEvent                            │                          │                          │
     ├───────────────────────────>│                            │                          │                          │
     │                            │                            │                          │                          │
     │                            │ 3. 消费事件                │                          │                          │
     │                            ├───────────────────────────>│                          │                          │
     │                            │                            │ 4. FluentValidation 校验 │                          │
     │                            │                            │ (信用卡、邮箱等)          │                          │
     │                            │                            │                          │                          │
     │                            │                            │ 5. 写入 Orders 表        │                          │
     │                            │                            │ (SQL Server)             │                          │
     │                            │                            │                          │                          │
     │                            │                            │ 6. 写入 OutboxMessages   │                          │
     │                            │                            │ (Type=OrderCreatedEvent) │                          │
     │                            │                            │                          │                          │
     │ 202 Accepted               │                            │                          │                          │
     │<───────────────────────────│                            │                          │                          │
     │                            │                            │                          │                          │
     │                            │                            │ 7. OutboxDispatcher 轮询 │                          │
     │                            │                            │ 发布未处理消息            │                          │
     │                            │<───────────────────────────┤                          │                          │
     │                            │ OrderCreatedEvent          │                          │                          │
     │                            ├──────────────────────────────────────────────────────>│                          │
     │                            │                            │                          │ 8. 消费事件              │
     │                            │                            │                          ├─────────────────────────>│
     │                            │                            │                          │                          │
     │                            │                            │                          │                          │ 9. 处理支付
     │                            │                            │                          │                          │ (Task.Delay)
     │                            │                            │                          │                          │
     │                            │                            │                          │                          │ 10. 发布 PaymentCompleted
     │                            │                            │                          │                          ├───>│
     │                            │                            │                          │                          │
```

#### 2.3 关键状态转换

```
订单状态 (OrderStatus):
┌──────────────┐   创建订单   ┌──────────────┐   支付完成   ┌──────────────┐
│   (未存在)   │ ──────────> │   Pending    │ ──────────> │  Completed   │
└──────────────┘             └──────┬───────┘             └──────────────┘
                                    │
                                    │ 支付失败
                                    ▼
                             ┌──────────────┐
                             │   Failed     │
                             └──────────────┘

Outbox 消息状态:
┌──────────────┐  Dispatcher  ┌──────────────┐  发布成功   ┌──────────────┐
│  OccurredOn  │ ──────────> │  ProcessedOn  │ ──────────> │   NULL (已处理)│
│  ErrorMsg=   │             │  = NULL       │             │              │
│  NULL        │             └──────┬───────┘             └──────────────┘
└──────────────┘                    │ 失败
                                    ▼
                             ┌──────────────┐
                             │  ErrorMsg =  │
                             │  错误详情     │
                             └──────────────┘
```

---

### 3. 接口请求流程文档

所有接口通过 API Gateway (`http://localhost:8010`) 访问。除标识为「公开」的接口外，其他需要 JWT 鉴权的接口必须在请求头携带：

```
Authorization: Bearer <token>
```

#### 3.1 Identity 服务接口（身份认证）

##### 3.1.1 用户注册

```
POST /identity/api/auth
Content-Type: application/json
```

**请求参数：**
```json
{
  "name": "张三",
  "email": "zhangsan@example.com",
  "password": "Pass@word1"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 用户名 |
| email | string | 是 | 邮箱（作为登录账号） |
| password | string | 是 | 密码（需包含大小写字母、数字、特殊字符） |

**响应：** `200 OK`
```json
{
  "message": "Registration successfully"
}
```

**请求流程：**
```
[Client] ──POST──> [Ocelot:8010] ──> [identity.api:8060/api/auth]
                                              │
                                              ▼
                                      [ASP.NET Identity]
                                      写入 IdentityDb
                                              │
                                              ▼
                                        200 OK
```

##### 3.1.2 用户登录（获取 JWT）

```
POST /identity/api/auth/login
Content-Type: application/json
```

**请求参数：**
```json
{
  "email": "zhangsan@example.com",
  "password": "Pass@word1"
}
```

**响应：** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ6..."
}
```

**Token 结构（JWT Payload）：**
```json
{
  "sub": "zhangsan@example.com",
  "unique_name": "zhangsan@example.com",
  "uid": "<user-guid>",
  "iss": "learn-dotnet-ecommerce-microservices",
  "aud": "learn-dotnet-ecommerce-microservices",
  "exp": 1756355200
}
```

**请求流程：**
```
[Client] ──POST──> [Ocelot:8010] ──> [identity.api:8060/api/auth/login]
                                              │
                                              ▼
                                      [UserManager 查询 IdentityDb]
                                              │ 校验密码
                                              ▼
                                      [生成 JWT (HS256)]
                                              │
                                              ▼
                                      200 OK + { token }
```

---

#### 3.2 Catalog 服务接口（商品目录）

##### 3.2.1 获取所有商品（分页 + 过滤）

```
GET /Catalog/GetAllProducts?pageIndex=1&pageSize=10&brand=602d2149e773f2a3990b47f6
```

**查询参数：**

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| pageIndex | int | 1 | 页码 |
| pageSize | int | 10 | 每页条数（最大 70） |
| brandId | string | - | 品牌过滤 |
| typeId | string | - | 类型过滤 |
| sort | string | - | 排序字段 |
| search | string | - | 搜索关键字 |

**响应：** `200 OK`
```json
[
  {
    "id": "602d2149e773f2a3990b47f8",
    "name": "Adidas Quick Force Indoor Badminton Shoes",
    "summary": "Professional badminton shoes",
    "description": "Description text",
    "imageFile": "product-1.png",
    "brand": {
      "id": "602d2149e773f2a3990b47f6",
      "name": "Adidas"
    },
    "type": {
      "id": "602d2149e773f2a3990b47f2",
      "name": "Shoes"
    },
    "price": 1000.00,
    "createdDate": "2026-06-29T10:00:00Z"
  }
]
```

**请求流程：**
```
[Client] ──GET──> [Ocelot:8010] ──> [catalog.api:8000/api/v1/Catalog/GetAllProducts]
                                              │
                                              ▼
                                      [MediatR GetAllProductsQuery]
                                              │
                                              ▼
                                      [MongoDB Aggregation]
                                      (join brands + types)
                                              │
                                              ▼
                                        200 OK
```

##### 3.2.2 根据 ID 获取商品

```
GET /Catalog/{id}
```

**路径参数：** `id` - MongoDB ObjectId（24 位十六进制字符串）

**响应：** `200 OK`（同上单条商品对象）；若不存在 `404 Not Found`

##### 3.2.3 根据品牌获取商品

```
GET /Catalog/brand/{brand}
```

**路径参数：** `brand` - 品牌 ID

##### 3.2.4 获取所有品牌

```
GET /Catalog/GetAllBrands
```

**响应：** `200 OK`
```json
[
  { "id": "602d2149e773f2a3990b47f6", "name": "Adidas" },
  { "id": "602d2149e773f2a3990b47f7", "name": "Yonex" }
]
```

##### 3.2.5 获取所有类型

```
GET /Catalog/GetAllTypes
```

**响应：** `200 OK`
```json
[
  { "id": "602d2149e773f2a3990b47f2", "name": "Shoes" },
  { "id": "602d2149e773f2a3990b47f3", "name": "Racquet" }
]
```

##### 3.2.6 根据商品名称搜索商品

```
GET /Catalog/productName/{productName}
```

**路径参数：** `productName` - 商品名称（URL 编码，支持空格等特殊字符）

**响应：** `200 OK`（返回匹配的商品列表）；若无匹配返回 `404 Not Found`
```json
[
  {
    "id": "602d2149e773f2a3990b47f8",
    "name": "Adidas Quick Force Indoor Badminton Shoes",
    "price": 3500.00
  }
]
```

**请求流程：**
```
[Client] ──GET──> [Ocelot:8010] ──> [catalog.api:8000/api/v1/Catalog/productName/{productName}]
                                              │
                                              ▼
                                      [MediatR GetProductsByProductNameQuery]
                                              │
                                              ▼
                                      [MongoDB: find Name = @productName]
                                              │
                                              ▼
                                        200 OK / 404
```

##### 3.2.7 创建商品

```
POST /Catalog
Content-Type: application/json
```

**请求参数：**
```json
{
  "name": "New Badminton Racket",
  "summary": "Professional grade racket",
  "description": "Carbon fiber frame, lightweight design",
  "imageFile": "new-racket.png",
  "brandId": "602d2149e773f2a3990b47f6",
  "typeId": "602d2149e773f2a3990b47f3",
  "price": 1500.00
}
```

| 字段 | 类型 | 必填 | 校验规则 |
|------|------|------|----------|
| name | string | 是 | 非空 |
| summary | string | 是 | 非空 |
| description | string | 是 | 非空 |
| imageFile | string | 是 | 非空 |
| brandId | string | 是 | 有效的品牌 ID |
| typeId | string | 是 | 有效的类型 ID |
| price | decimal | 是 | ≥ 0.01 |

**响应：** `200 OK`（返回新创建的商品对象，包含生成的 `id`）

**请求流程：**
```
[Client] ──POST──> [Ocelot:8010] ──> [catalog.api:8000/api/v1/Catalog]
                                              │
                                              ▼
                                      [MediatR CreateProductCommand]
                                              │
                                              ▼
                                      [校验 Brand + Type 存在]
                                      [MongoDB: INSERT Products]
                                              │
                                              ▼
                                        200 OK + ProductResponse
```

##### 3.2.8 更新商品

```
PUT /Catalog/{id}
Content-Type: application/json
```

**路径参数：** `id` - MongoDB ObjectId

**请求参数：**
```json
{
  "name": "Updated Racket Name",
  "summary": "Updated summary",
  "description": "Updated description",
  "imageFile": "updated.png",
  "brandId": "602d2149e773f2a3990b47f6",
  "typeId": "602d2149e773f2a3990b47f3",
  "price": 1200.00
}
```

**响应：** `204 No Content`（成功）；若商品不存在返回 `404 Not Found`

##### 3.2.9 删除商品

```
DELETE /Catalog/{id}
```

**路径参数：** `id` - MongoDB ObjectId（24 位十六进制字符串）

**响应：** `204 No Content`（成功）；若商品不存在返回 `404 Not Found`

**请求流程：**
```
[Client] ──DELETE──> [Ocelot:8010] ──> [catalog.api:8000/api/v1/Catalog/{id}]
                                              │
                                              ▼
                                      [MediatR DeleteProductByIdCommand]
                                              │
                                              ▼
                                      [MongoDB: DELETE Products]
                                      WHERE _id = ObjectId(id)
                                              │
                                              ▼
                                        204 / 404
```

---

#### 3.3 Basket 服务接口（购物车）

##### 3.3.1 获取购物车

```
GET /Basket/{userName}
```

**路径参数：** `userName` - 用户名

**响应：** `200 OK`
```json
{
  "userName": "zhangsan@example.com",
  "items": [
    {
      "productId": "602d2149e773f2a3990b47f8",
      "productName": "Adidas Quick Force Indoor Badminton Shoes",
      "imageFile": "product-1.png",
      "price": 500.00,
      "quantity": 1
    }
  ],
  "totalPrice": 500.00
}
```

##### 3.3.2 创建/更新购物车（**gRPC 集成**）

```
POST /Basket
Content-Type: application/json
```

**请求参数：**
```json
{
  "userName": "zhangsan@example.com",
  "items": [
    {
      "productId": "602d2149e773f2a3990b47f8",
      "productName": "Adidas Quick Force Indoor Badminton Shoes",
      "imageFile": "product-1.png",
      "price": 1000.00,
      "quantity": 1
    }
  ]
}
```

**响应：** `200 OK`（返回应用折扣后的购物车，`price` 已更新）

**请求流程（含 gRPC 调用）：**
```
[Client] ──POST──> [Ocelot:8010] ──> [basket.api:8020/api/v1/Basket]
                                              │
                                              ▼
                                  [CreateShoppingCartHandler]
                                              │
                                              │ 遍历每个商品
                                              ▼
                                  ┌────────────────────────────┐
                                  │ DiscountGrpcService        │
                                  │  .GetDiscount(productName) │
                                  └────────────┬───────────────┘
                                               │ gRPC (HTTP/2)
                                               ▼
                                  [discount.api:8030]
                                  [GetDiscountQuery]
                                  [PostgreSQL: SELECT amount]
                                               │
                                               │ 返回 CouponModel
                                               ▼
                                  [price -= coupon.Amount]
                                  (1000 - 500 = 500)
                                               │
                                               ▼
                                  [Redis: SET basket:userName]
                                               │
                                               ▼
                                       200 OK + 购物车
```

##### 3.3.3 删除购物车

```
DELETE /Basket/{userName}
```

**响应：** `200 OK`（返回 `true` / `false`）

##### 3.3.4 结账（**JWT 鉴权 + 事件发布**）

```
POST /Basket/Checkout
Authorization: Bearer <token>
Content-Type: application/json
```

**请求参数：**
```json
{
  "userName": "zhangsan@example.com",
  "totalPrice": 500.00,
  "name": "张三",
  "emailAddress": "zhangsan@example.com",
  "addressLine": "南京路 100 号",
  "country": "CN",
  "state": "Shanghai",
  "zipCode": "200000",
  "cardName": "VISA",
  "cardNumber": "4111111111111111",
  "cardExpiration": "12/30",
  "cvv": "123",
  "paymentMethod": 1
}
```

| 字段 | 类型 | 必填 | 校验规则 |
|------|------|------|----------|
| cardNumber | string | 是 | 须通过 Luhn 算法校验（测试卡 `4111111111111111`） |
| cardExpiration | string | 是 | 格式 `MM/YY` |
| cvv | string | 是 | 3 或 4 位数字 |
| totalPrice | decimal | 是 | ≥ 0 |

**响应：** `202 Accepted`（无响应体）

**请求流程（触发事件驱动 Saga）：**
```
[Client] ──POST (JWT)──> [Ocelot:8010]
   ├──鉴权成功────────> [basket.api:8020/api/v1/Basket/Checkout]
   │                          │
   │                          ▼
   │                  [BasketCheckoutHandler]
   │                          │
   │                          ▼
   │                  [MassTransit Publish]
   │                  BasketCheckoutEvent
   │                          │
   │                          ▼
   │                  [RabbitMQ: basket-checkout-queue]
   │                          │
   │                          ▼ (异步)
   │                  [Ordering.api 消费事件]
   │                  [CreateOrderHandler]
   │                  [FluentValidation]
   │                  [写入 SQL Server + Outbox]
   │                          │
   │                          ▼ (异步)
   │                  [OutboxDispatcher]
   │                  OrderCreatedEvent
   │                          │
   │                          ▼
   │                  [RabbitMQ: order-created-queue]
   │                          │
   │                          ▼ (异步)
   │                  [Payment.api 消费事件]
   │                  [处理支付 → 发布 PaymentCompletedEvent]
   │
   └──> 立即返回 202 Accepted
```

---

#### 3.4 Ordering 服务接口（订单管理，**JWT 鉴权**）

##### 3.4.1 获取用户订单

```
GET /Order/{userName}
Authorization: Bearer <token>
```

**响应：** `200 OK`
```json
[
  {
    "id": 1002,
    "userName": "zhangsan@example.com",
    "totalPrice": 500.00,
    "name": "张三",
    "emailAddress": "zhangsan@example.com",
    "addressLine": "南京路 100 号",
    "country": "CN",
    "state": "Shanghai",
    "zipCode": "200000",
    "cardName": "VISA",
    "cardNumber": "4111111111111111",
    "cardExpiration": "12/30",
    "cvv": "123",
    "paymentMethod": 1
  }
]
```

**请求流程：**
```
[Client] ──GET (JWT)──> [Ocelot:8010]
   │   JWT 鉴权         │
   ▼                    ▼
[AuthenticationMiddleware] ──> [ordering.api:8040/api/v1/Order/{userName}]
                                       │
                                       ▼
                              [IQueryHandler<GetOrderListQuery>]
                                       │
                                       ▼
                              [EF Core 查询 SQL Server]
                              WHERE UserName = @userName
                                       │
                                       ▼
                                  200 OK
```

##### 3.4.2 创建订单

```
POST /Order
Authorization: Bearer <token>
Content-Type: application/json
```

**请求参数：** 同 `BasketCheckoutDto` 的字段集合

**响应：** `200 OK`
```json
1002   // 返回新创建的订单 ID
```

##### 3.4.3 更新订单

```
PUT /Order
Authorization: Bearer <token>
Content-Type: application/json
```

**请求参数：** `OrderDto`（包含 `Id` 字段）

##### 3.4.4 删除订单

```
DELETE /Order/{id}
Authorization: Bearer <token>
```

**响应：** `204 No Content`

---

#### 3.5 Discount 服务接口（gRPC）

Discount 服务通过 **gRPC（HTTP/2）** 暴露在 `discount.api:8080`，由 Basket.API 内部调用，**不通过 Ocelot 网关**。

**Proto 定义（`discount.proto`）：**
```protobuf
service DiscountProtoService {
  rpc GetDiscount(GetDiscountRequest) returns (CouponModel);
  rpc CreateDiscount(CreateDiscountRequest) returns (CouponModel);
  rpc UpdateDiscount(UpdateDiscountRequest) returns (CouponModel);
  rpc DeleteDiscount(DeleteDiscountRequest) returns (DeleteDiscountResponse);
}

message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}
```

**调用示例（来自 Basket.API）：**
```csharp
var request = new GetDiscountRequest { ProductName = "Adidas Quick Force Indoor Badminton Shoes" };
var coupon = await discountProtoServiceClient.GetDiscountAsync(request);
// coupon.Amount = 500
// item.Price -= coupon.Amount;  → 1000 - 500 = 500
```

**数据库中的折扣券数据：**
```sql
SELECT * FROM coupon;
-- id | productname                                         | description       | amount
----+-----------------------------------------------------+-------------------+-------
--  1 | Adidas Quick Force Indoor Badminton Shoes           | Shoe Discount     |    500
--  2 | Yonex VCORE Pro 100 A Tennis Racquet (270gm, Strung)| Racquet Discount  |    700
```

##### 3.5.1 GetDiscount - 查询折扣券

**请求：** `GetDiscountRequest`
```protobuf
message GetDiscountRequest {
  string productName = 1;   // 商品名称（精确匹配）
}
```

**响应：** `CouponModel`
```protobuf
message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;          // 折扣金额（分），从商品原价中扣除
}
```

**调用示例：**
```csharp
var request = new GetDiscountRequest { ProductName = "Adidas Quick Force Indoor Badminton Shoes" };
var coupon = await discountProtoServiceClient.GetDiscountAsync(request);
// coupon.Amount = 500
// item.Price -= coupon.Amount;  → 1000 - 500 = 500
```

**请求流程：**
```
[Basket.API] ──gRPC──> [discount.api:8080]
       GetDiscountRequest(productName)
                                │
                                ▼
                       [MediatR GetDiscountQuery]
                                │
                                ▼
                       [PostgreSQL: SELECT FROM coupon
                        WHERE productname = @productName]
                                │
                                ▼
                       返回 CouponModel (amount=500)
```

##### 3.5.2 CreateDiscount - 创建折扣券

**请求：** `CreateDiscountRequest`
```protobuf
message CreateDiscountRequest {
  CouponModel coupon = 1;    // 待创建的折扣券对象
}
```

**调用示例：**
```csharp
var request = new CreateDiscountRequest
{
    Coupon = new CouponModel
    {
        ProductName = "New Product Name",
        Description = "10% off promotion",
        Amount = 100
    }
};
var created = await discountProtoServiceClient.CreateDiscountAsync(request);
// created.Id = 新生成的 ID
```

**响应：** `CouponModel`（包含新生成的 `id`）

**请求流程：**
```
[Client] ──gRPC──> [discount.api:8080]
       CreateDiscountRequest(coupon)
                          │
                          ▼
                  [MediatR CreateDiscountCommand]
                          │
                          ▼
                  [PostgreSQL: INSERT INTO coupon
                   (productname, description, amount)
                   VALUES (@productName, @description, @amount)]
                          │
                          ▼
                  返回 CouponModel (含新 id)
```

##### 3.5.3 UpdateDiscount - 更新折扣券

**请求：** `UpdateDiscountRequest`
```protobuf
message UpdateDiscountRequest {
  CouponModel coupon = 1;    // 待更新的折扣券（需包含 id）
}
```

**调用示例：**
```csharp
var request = new UpdateDiscountRequest
{
    Coupon = new CouponModel
    {
        Id = 1,
        ProductName = "Adidas Quick Force Indoor Badminton Shoes",
        Description = "Updated: 20% off",
        Amount = 200
    }
};
var updated = await discountProtoServiceClient.UpdateDiscountAsync(request);
```

**响应：** `CouponModel`（更新后的对象）

##### 3.5.4 DeleteDiscount - 删除折扣券

**请求：** `DeleteDiscountRequest`
```protobuf
message DeleteDiscountRequest {
  string productName = 1;   // 通过商品名称删除
}
```

**调用示例：**
```csharp
var request = new DeleteDiscountRequest { ProductName = "Old Product Name" };
var response = await discountProtoServiceClient.DeleteDiscountAsync(request);
// response.Success = true / false
```

**响应：** `DeleteDiscountResponse`
```protobuf
message DeleteDiscountResponse {
  bool success = 1;          // 是否删除成功
}
```

**请求流程：**
```
[Client] ──gRPC──> [discount.api:8080]
       DeleteDiscountRequest(productName)
                          │
                          ▼
                  [MediatR DeleteDiscountCommand]
                          │
                          ▼
                  [PostgreSQL: DELETE FROM coupon
                   WHERE productname = @productName]
                          │
                          ▼
                  返回 DeleteDiscountResponse (success=true/false)
```

---

### 4. 完整端到端请求时序

以下展示「从注册到完成支付」的完整请求时序：

```
时间线    客户端              Ocelot           微服务            数据库/中间件
  │
  │  1. POST /identity/api/auth ───────────────────────> IdentityDb (INSERT)
  │     body: {name,email,password}
  │  <─ 200 { message }
  │
  │  2. POST /identity/api/auth/login ──────────────────> IdentityDb (SELECT)
  │     body: {email,password}
  │  <─ 200 { token: "eyJ..." }
  │
  │  3. GET /Catalog/GetAllProducts ────────────────────> MongoDB (find)
  │  <─ 200 [ {id,name,...}, ... ]
  │
  │  4. POST /Basket ────────────────────────────────────> Redis (SET)
  │     body: {userName, items:[{price:1000}]}
  │                                  │
  │                                  ├─ gRPC ────────> discount.api
  │                                  │                PostgreSQL (SELECT)
  │                                  │ <─ {amount:500}
  │                                  │ (price = 1000 - 500)
  │  <─ 200 { items:[{price:500}] }
  │
  │  5. POST /Basket/Checkout ───────────────────────────> MassTransit Publish
  │     Authorization: Bearer <token>                    │
  │     body: { totalPrice:500, cardNumber:"4111..." }   ▼
  │                                       RabbitMQ (basket-checkout-queue)
  │                                                  │
  │  <─ 202 Accepted                                ▼ (异步消费)
  │                                          Ordering.api
  │                                          - FluentValidation 校验
  │                                          - INSERT INTO Orders
  │                                          - INSERT INTO OutboxMessages
  │                                                  │
  │                                                  ▼ (OutboxDispatcher 轮询)
  │                                          RabbitMQ (order-created-queue)
  │                                                  │
  │                                                  ▼ (异步消费)
  │                                          Payment.api
  │                                          - Task.Delay(1000)
  │                                          - 模拟支付完成
  │                                          - Publish PaymentCompletedEvent
  │
  │  6. GET /Order/{email} (Bearer JWT) ────────────────> SQL Server (SELECT)
  │  <─ 200 [ {id:1002, totalPrice:500, ...} ]
  │
  ▼
```

### 5. 端口与容器映射速查

| 容器名 | 内部端口 | 外部端口 | 数据库 |
|--------|----------|----------|--------|
| ocelot.apigateway | 8080 | 8010 | - |
| catalog.api | 8080 | 8000 | catalog.db (MongoDB:27017) |
| basket.api | 8080 | 8020 | basket.db (Redis:6379) |
| discount.api | 8080 | 8030 | discount.db (Postgres:5432) |
| ordering.api | 8080 | 8040 | ordering.db (SQLServer:1433) |
| payment.api | 8080 | 8050 | - |
| identity.api | 8080 | 8060 | identity.db (SQLServer:1434) |
| rabbitmq | 5672 / 15672 | 5672 / 15672 | - |
| elasticsearch | 9200 | 9200 | - |
| kibana | 5601 | 5601 | - |