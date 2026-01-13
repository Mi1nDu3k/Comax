using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ComaxDbContext context) : base(context) { }

  
        public async Task<List<Comment>> GetParentsByComicAsync(int comicId, int page, int pageSize)
        {
            return await _dbSet
                .Include(c => c.User) 
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .Where(c => c.ComicId == comicId && c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 2. Lấy Comment Con (ParentId == id cha)
        public async Task<List<Comment>> GetRepliesAsync(int parentId, int page, int pageSize)
        {
            return await _dbSet
                .Include(c => c.User)
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.CreatedAt) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}