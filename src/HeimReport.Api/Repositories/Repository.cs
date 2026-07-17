using HeimReport.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{
    protected ApplicationDbContext Context { get; } = context;

    protected DbSet<T> DbSet { get; } = context.Set<T>();

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet.FindAsync([id], cancellationToken).AsTask();
    }

    public IQueryable<T> Query()
    {
        return DbSet;
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}