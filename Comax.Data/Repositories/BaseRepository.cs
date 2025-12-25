using Comax.Data;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ComaxDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ComaxDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }
    // THÊM TỪ KHÓA 'virtual' VÀO ĐÂY
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    public IQueryable<T> FindAll(bool trackChanges) =>
        !trackChanges ?
          _context.Set<T>().AsNoTracking() :
          _context.Set<T>();
    public async Task<T> AddAsync(T entity)
    {
        if (entity is Comax.Data.Entities.BaseEntity baseEntity)
        {
            baseEntity.RowVersion = Guid.NewGuid();
        }
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public async Task<T?> UpdateAsync(T entity)
    {
        if (entity is Comax.Data.Entities.BaseEntity baseEntity)
        {
            baseEntity.RowVersion = Guid.NewGuid();
        }

        return entity;
    }

    public async Task<(List<T> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> DeleteAsync(int id, bool hardDelete = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        if (hardDelete)
        {
            _dbSet.Remove(entity);
        }
        else
        {
            if (entity is Comax.Data.Entities.BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.DeletedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }
        return true;
    }
}