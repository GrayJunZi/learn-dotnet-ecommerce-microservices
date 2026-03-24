namespace Basket.Application.Responses;

public record class ShoppingCartItemResponse
{
    public int Quantity { get; init; }
    public string ImageFile { get; set; }
    public decimal Price { get; init; }
    public string ProductId { get; init; }
    public string ProductName { get; init; }
}

public record class ShoppingCartResponse
{
    public string UserName { get; init; }
    public IEnumerable<ShoppingCartItemResponse> Items { get; init; }

    public ShoppingCartResponse()
    {
        UserName = string.Empty;
        Items = [];
    }

    public ShoppingCartResponse(string userName) : this(userName, [])
    {
    }

    public ShoppingCartResponse(string userName, IEnumerable<ShoppingCartItemResponse> items)
    {
        UserName = userName ?? string.Empty;
        Items = items ?? [];
    }
    
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}