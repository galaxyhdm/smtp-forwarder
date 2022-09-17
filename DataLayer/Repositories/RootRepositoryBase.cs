using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.DataLayer.Events;

namespace SmtpForwarder.DataLayer.Repositories;

internal abstract class RootRepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;

    protected event EventHandler<SavingEventArgs<TEntity>>? BeforeAdd;
    protected event EventHandler<SavingEventArgs<TEntity>>? BeforeUpdate;
    protected event EventHandler<SavingEventArgs<TEntity>>? BeforeSave;
    protected event EventHandler<SavingEventArgs<TEntity>>? AfterSave;
    protected event EventHandler<SavingFailedEventArgs<TEntity>>? SaveFailed;

    protected IQueryable<TEntity> Entities { get; init; }

    protected RootRepositoryBase(DbContext context)
    {
        _context = context;
        Entities = context.Set<TEntity>().AsNoTracking().AsQueryable();
    }

    public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate) =>
        await Entities.SingleOrDefaultAsync(predicate);

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await Entities.ToListAsync();

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate) =>
        await Entities.Where(predicate).ToListAsync();

    public virtual async Task<bool> AddAsync(TEntity entity)
    {
        BeforeAdd?.Invoke(this, new SavingEventArgs<TEntity>(entity));
        await _context.AddAsync(entity);
        var result = await SaveAsync(entity);
        return result;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        BeforeUpdate?.Invoke(this, new SavingEventArgs<TEntity>(entity));
        _context.Update(entity);
        var result = await SaveAsync(entity);
        return result;
    }

    protected virtual async Task<bool> SaveAsync(TEntity entity)
    {
        BeforeSave?.Invoke(this, new SavingEventArgs<TEntity>(entity));

        try
        {
            await _context.SaveChangesAsync();
            AfterSave?.Invoke(this, new SavingEventArgs<TEntity>(entity));

            return true;
        }
        catch (DbUpdateException e)
        {
            SaveFailed?.Invoke(this, new SavingFailedEventArgs<TEntity>(entity, e));
            return false;
        }
        
    }
}