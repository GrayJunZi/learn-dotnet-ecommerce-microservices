using Microsoft.EntityFrameworkCore;
using Ordering.Core.Entities;

namespace Ordering.Infrastructure.Data;

public class OrderContext(DbContextOptions<OrderContext> options) : DbContext(options)
{
    public DbSet<Order>  Orders { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate =  DateTime.UtcNow;
                    entry.Entity.CreatedBy = "";
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedDate =  DateTime.UtcNow;
                    entry.Entity.ModifiedBy = "";
                    break;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}