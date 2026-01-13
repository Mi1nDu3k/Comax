using Comax.Common.DTOs.Pagination;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task AddRangeAsync(IEnumerable<T> entities); 
        Task<List<T>> GetAllAsync();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);        
        Task<T?> UpdateAsync(T entity);    
        Task<bool> DeleteAsync(int id, bool hardDelete = false);
        Task<(List<T> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
        void Update(T entity); 


    }
}