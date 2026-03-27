using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Discount.Infrastructure.Settings;

public static class DbExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        // var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        // var logger = loggerFactory.CreateLogger("DbExtensions");
        var logger = services.GetRequiredService<ILogger>();
        var databaseSettings = services.GetRequiredService<IOptions<DatabaseSettings>>().Value;

        try
        {
            logger.LogInformation("Discount Db Migration Started.");
            ApplyMigration(databaseSettings.ConnectionString);
            logger.LogInformation("Discount Db Migration Completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }

        return host;
    }

    private static void ApplyMigration(string connectionString)
    {
        var retry = 5;
        do
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using var command = new NpgsqlCommand
                {
                    Connection = connection
                };

                command.CommandText = "DROP  TABLE IF EXISTS Coupon";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE Coupon (
                        Id SERIAL PRIMARY KEY,
                        ProductName VARCHAR(500) NOT NULL,
                        Description TEXT,
                        Amount INT
                    )";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    INSERT INTO Coupon (ProductName, Description, Amount) 
                    VALUES ('Adidas Quick Force Indoor Badminton Shoes', 'Shoe Discount', 500)";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    INSERT INTO Coupon (ProductName, Description, Amount) 
                    VALUES ('Yonex VCORE Pro 100 A Tennis Racquet (270gm, Strung)', 'Racquet Discount', 700)";
                command.ExecuteNonQuery();
                break;
            }
            catch
            {
                retry--;
                if (retry == 0)
                {
                    throw;
                }
            }
        } while (retry > 0);
    }
}