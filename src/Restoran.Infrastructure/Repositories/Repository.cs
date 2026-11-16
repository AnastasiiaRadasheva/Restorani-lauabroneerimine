using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

/// <summary>Üldine EF Core repositoorium.</summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;

    public Repository(AppDbContext db)
    {
        Db = db;
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await Db.Set<T>().FindAsync(id);

    public async Task AddAsync(T entity) => await Db.Set<T>().AddAsync(entity);

    public void Remove(T entity) => Db.Set<T>().Remove(entity);

    public Task SaveChangesAsync() => Db.SaveChangesAsync();
}
