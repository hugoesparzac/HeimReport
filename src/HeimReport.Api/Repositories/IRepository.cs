namespace HeimReport.Api.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    IQueryable<T> Query();

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}