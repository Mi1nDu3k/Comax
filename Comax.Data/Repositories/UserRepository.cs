using Comax.Data;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly ComaxDbContext _context;
    private readonly DbSet<User> _dbSet;

    public UserRepository(ComaxDbContext context) : base(context)
    {
        _context = context;
        _dbSet = _context.Set<User>();
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public override async Task<User?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Role) 
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> AddAsync(User entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<User?> UpdateAsync(User entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<User?> GetByIdWithRoleAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role) 
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

    }
    public async Task<List<User>> GetByRoleIdAsync(int roleId)
    {
        return await _context.Users
            .Include(u => u.Role) // Include Role để lấy tên Role hiển thị ra DTO
            .Where(u => u.RoleId == roleId)
            .ToListAsync();
    }
}
