using System.Linq.Expressions;
using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BroomErp.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        if (predicate != null)
            query = query.Where(predicate);

        foreach (var include in includes)
            query = query.Include(include);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> GetPagedAsync(QueryParams queryParams, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        if (predicate != null)
            query = query.Where(predicate);

        foreach (var include in includes)
            query = query.Include(include);

        var total = await query.CountAsync(cancellationToken);

        query = queryParams.Order.ToLower() == "asc"
            ? query.OrderBy(e => e.Id)
            : query.OrderByDescending(e => e.Id);

        var skip = Math.Max(0, (queryParams.Page - 1) * queryParams.PageSize);
        var items = await query.Skip(skip).Take(queryParams.PageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
