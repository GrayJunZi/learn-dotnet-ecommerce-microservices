namespace Basket.Core.Entities;

public class BasketCheckout
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string CardExpiration { get; set; }
    public string Cvv { get; set; }
    public string PaymentMethod { get; set; }
}