using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using Ordering.Infrastructure.Data;

namespace Ordering.Infrastructure.Repositories;

public class RepositoryBase<T>(OrderContext orderContext) : IAsyncRepository<T> where T : EntityBase
{
    public async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = orderContext.Set<T>().AsNoTracking();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await orderContext.Set<T>().FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        orderContext.Set<T>().Add(entity);
        await orderContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        orderContext.Entry(entity).State = EntityState.Modified;
        await orderContext.SaveChangesAsync();
    }


    public async Task DeleteAsync(T entity)
    {
        orderContext.Set<T>().Remove(entity);
        await orderContext.SaveChangesAsync();
    }
}