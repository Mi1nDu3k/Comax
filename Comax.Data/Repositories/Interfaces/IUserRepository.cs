using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRoleAsync(int id);
    Task<List<User>> GetByRoleIdAsync(int roleId);
}