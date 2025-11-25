using Comax.Data;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity; // phải trả entity
    }

    public async Task<T?> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        return entity; // phải trả entity
    }

    public async Task<(List<T> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.AsQueryable();

        // 1. Đếm tổng số lượng (trước khi Skip/Take)
        var totalCount = await query.CountAsync();

        // 2. Phân trang
        var items = await query
            .Skip((pageNumber - 1) * pageSize) // Bỏ qua các item của trang trước
            .Take(pageSize)                   // Chỉ lấy số item của trang hiện tại
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> DeleteAsync(int id, bool hardDelete = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        if (hardDelete)
        {
            // Xóa vĩnh viễn
            _dbSet.Remove(entity);
        }
        else
        {
            // Soft Delete: Chỉ thực hiện nếu entity có kế thừa BaseEntity
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.DeletedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            else
            {
                // Nếu entity không hỗ trợ soft delete (không có IsDeleted), buộc phải xóa cứng hoặc báo lỗi
                _dbSet.Remove(entity);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
