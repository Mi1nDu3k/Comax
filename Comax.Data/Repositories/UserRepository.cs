using Comax.Data;
using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly ComaxDbContext _context;
    public UserRepository(ComaxDbContext context) => _context = context;

    public async Task AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Include(u => u.Role).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }
}
