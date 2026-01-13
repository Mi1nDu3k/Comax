using Comax.Data;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
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
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        if (entity is Comax.Data.Entities.BaseEntity baseEntity)
        {
            // Gán giá trị mặc định khi tạo mới
            baseEntity.RowVersion = Guid.NewGuid();
            // Nếu có CreatedAt, nên gán luôn ở đây
            baseEntity.CreatedAt = DateTime.UtcNow;
        }

        await _dbSet.AddAsync(entity); // Đã đúng
        return entity;
    }

    // --- ĐÃ SỬA LẠI HÀM NÀY ---
    public async Task<T?> UpdateAsync(T entity)
    {
        if (entity is Comax.Data.Entities.BaseEntity baseEntity)
        {
            baseEntity.RowVersion = Guid.NewGuid();
            // baseEntity.UpdatedAt = DateTime.UtcNow; // Nên có dòng này nếu BaseEntity hỗ trợ
        }

        _dbSet.Update(entity); // <--- QUAN TRỌNG NHẤT: Báo cho EF biết cần update
        await Task.CompletedTask; // Giữ nguyên signature async

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

    public virtual async Task<bool> DeleteAsync(int id, bool hardDelete = false)
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
                _dbSet.Update(entity); // Soft delete cũng cần gọi Update (đoạn này bạn viết đúng rồi)
            }
            else
            {
                // Nếu entity không kế thừa BaseEntity thì bắt buộc phải hard delete
                _dbSet.Remove(entity);
            }
        }
        return true;
    }
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}