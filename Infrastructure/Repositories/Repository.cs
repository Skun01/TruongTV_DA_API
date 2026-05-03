using Application.IRepositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    internal DbSet<T> dbSet;
    public Repository(AppDbContext context)
    {
        _context = context;
        dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public void DeleteAsync(T entity)
    {
        dbSet.Remove(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await dbSet.FindAsync(id);
    }

    public void UpdateAsync(T entity)
    {
        dbSet.Update(entity);
    }

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null
            ? await dbSet.CountAsync()
            : await dbSet.CountAsync(predicate);
    }
}
