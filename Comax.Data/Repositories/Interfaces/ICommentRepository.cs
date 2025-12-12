using Comax.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
 
        Task<List<Comment>> GetParentsByComicAsync(int comicId, int page, int pageSize);
        Task<List<Comment>> GetRepliesAsync(int parentId, int page, int pageSize);
    }
}