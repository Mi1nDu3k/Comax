using Comax.Business.Interfaces;
using Comax.Common.DTOs.Comment;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        public CommentService(ICommentRepository repo) { _repo = repo; }

        public async Task<Comment> CreateAsync(CommentCreateDTO dto)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                ComicId = dto.ComicId,
                UserId = dto.UserId
            };
            return await _repo.AddAsync(comment);
        }

        public async Task<Comment?> UpdateAsync(int id, CommentUpdateDTO dto)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) return null;
            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            return await _repo.UpdateAsync(comment);
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<List<Comment>> GetByComicAsync(int comicId) => await _repo.GetByComicAsync(comicId);
    }
}

