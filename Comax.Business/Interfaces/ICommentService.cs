using Comax.Common.DTOs.Comment;
using Comax.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface ICommentService : IBaseService<CommentDTO, CommentCreateDTO, CommentUpdateDTO>
    {
        Task<List<CommentDTO>> GetByComicAsync(int comicId);
        Task<List<CommentDTO>> GetParentsByComicAsync(int comicId, int page = 1);

        Task<List<CommentDTO>> GetRepliesAsync(int parentId, int page = 1);

    }
}
