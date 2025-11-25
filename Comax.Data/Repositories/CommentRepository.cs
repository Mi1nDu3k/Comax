using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<List<Comment>> GetByComicAsync(int comicId);
    }
}
