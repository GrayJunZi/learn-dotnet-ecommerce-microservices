using Dapper;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using Discount.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Discount.Infrastructure.Repositories;

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

        var sql = $"SELECT * FROM Coupon WHERE ProductName = @ProductName";
        var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(sql, new
        {
            ProductName = productName
        });

        return coupon ?? new Coupon
        {
            ProductName = "No Discount",
            Description = "No Discount Available",
            Amount = 0
        };
    }

    public async Task<bool> CreateDiscount(Coupon coupon)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = $"INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)";
        var affected = await connection.ExecuteAsync(sql, new
        {
            coupon.ProductName,
            coupon.Description,
            coupon.Amount,
        });

        return affected > 0;
    }

    public async Task<bool> UpdateDiscount(Coupon coupon)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql =
            $"UPDATE Coupon SET ProductName = @ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, coupon);
        
        return affected > 0;
    }

    public async Task<bool> DeleteDiscount(string productName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var sql = $"DELETE FROM Coupon WHERE ProductName = @ProductName";
        var affected = await connection.ExecuteAsync(sql, new
        {
            ProductName = productName
        });
        return affected > 0;
    }
}