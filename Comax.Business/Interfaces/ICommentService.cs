using Comax.Common.DTOs.Comment;
using Comax.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface ICommentService
    {
        Task<Comment> CreateAsync(CommentCreateDTO dto);
        Task<Comment?> UpdateAsync(int id, CommentUpdateDTO dto);
        // Cập nhật tham số
        Task<bool> DeleteAsync(int id, bool hardDelete = false);
        Task<List<Comment>> GetByComicAsync(int comicId);
    }
}
