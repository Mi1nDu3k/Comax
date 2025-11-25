using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ComaxDbContext context) : base(context) { }

        public async Task<List<Comment>> GetByComicAsync(int comicId)
        {
            return await _context.Comments
                .Where(c => c.ComicId == comicId)
                .ToListAsync();
        }
    }
}
