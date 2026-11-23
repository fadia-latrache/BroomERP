using System.Linq.Expressions;
using BroomErp.Application.Common.Models;
using BroomErp.Domain.Common;

namespace BroomErp.Application.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<PagedResult<T>> GetPagedAsync(QueryParams query, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    IQueryable<T> Query();
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
