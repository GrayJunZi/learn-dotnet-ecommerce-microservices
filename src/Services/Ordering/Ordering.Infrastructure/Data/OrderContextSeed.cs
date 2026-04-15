using Microsoft.Extensions.Logging;
using Ordering.Core.Entities;

namespace Ordering.Infrastructure.Data;

public class OrderContextSeed
{
    public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
    {
        if (!orderContext.Orders.Any())
        {
            orderContext.Orders.AddRange(GetOrders());
            await orderContext.SaveChangesAsync();
            logger.LogInformation("Ordering Database has been successfully seeded.");
        }
    }
    
    private static IEnumerable<Order> GetOrders()
    {
        return new Order[]
        {
            new()
            {
                UserName = "test",
                TotalPrice = 99.99m,
                Name = "Test User",
                EmailAddress = "test@email.com",
                AddressLine = "123 Test Street",
                Country = "United States",
                State = "California",
                ZipCode = "90210",
                CardName = "Test User",
                CardNumber = "4111111111111111",
                CardExpiration = "12/25",
                Cvv = "123",
                PaymentMethod = 1,
                ModifiedBy = "test",
                ModifiedDate =  DateTime.UtcNow
            },
            new()
            {
                UserName = "alice",
                TotalPrice = 149.99m,
                Name = "Alice Smith",
                EmailAddress = "alice@example.com",
                AddressLine = "456 Main Street",
                Country = "United States",
                State = "New York",
                ZipCode = "10001",
                CardName = "Alice Smith",
                CardNumber = "5555555555554444",
                CardExpiration = "06/24",
                Cvv = "456",
                PaymentMethod = 2
            },
            new()
            {
                UserName = "bob",
                TotalPrice = 299.99m,
                Name = "Bob Johnson",
                EmailAddress = "bob@example.com",
                AddressLine = "789 Oak Avenue",
                Country = "Canada",
                State = "Ontario",
                ZipCode = "M5V 2T6",
                CardName = "Bob Johnson",
                CardNumber = "378282246310005",
                CardExpiration = "03/26",
                Cvv = "789",
                PaymentMethod = 1
            },
            new()
            {
                UserName = "charlie",
                TotalPrice = 49.99m,
                Name = "Charlie Brown",
                EmailAddress = "charlie@example.com",
                AddressLine = "321 Pine Road",
                Country = "United Kingdom",
                State = "England",
                ZipCode = "SW1A 1AA",
                CardName = "Charlie Brown",
                CardNumber = "30569309025904",
                CardExpiration = "09/25",
                Cvv = "321",
                PaymentMethod = 3
            }
        };
    }
}