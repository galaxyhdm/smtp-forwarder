using System.Linq.Expressions;

namespace Application.Interfaces.Repositories;

public interface IRepository<TEntity> where  TEntity : class
{
    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
    public Task<IEnumerable<TEntity>> GetAllAsync();
    public Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
    public Task<bool> AddAsync(TEntity entity);
    public Task<bool> UpdateAsync(TEntity entity);
}