using Microsoft.EntityFrameworkCore;
using HeimReport.Api.Data;

namespace HeimReport.Api.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public IQueryable<T> GetAllAsync() => _dbSet.AsQueryable();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}