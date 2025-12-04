using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Comment;
using Comax.Common.Enums;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class CommentService : BaseService<Comment, CommentDTO, CommentCreateDTO, CommentUpdateDTO>, ICommentService
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IMemoryCache _cache;
        private readonly INotificationService _notiService;

        public CommentService(
            ICommentRepository repo,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            INotificationService notiService)
            : base(repo, unitOfWork, mapper) 
        {
            _commentRepo = repo;
            _cache = cache;
        }



        public override async Task<CommentDTO> CreateAsync(CommentCreateDTO dto)
        {
            var entity = _mapper.Map<Comment>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _commentRepo.AddAsync(entity);

            await _unitOfWork.CommitAsync();
            if (dto.ParentId.HasValue)
            {
                // Tìm comment cha để biết ai là người viết
                var parentComment = await _commentRepo.GetByIdAsync(dto.ParentId.Value);

                // Nếu cha tồn tại và người trả lời KHÁC người viết comment cha
                if (parentComment != null && parentComment.UserId != dto.UserId)
                {
                    // Lấy thông tin người trả lời (để hiển thị tên)
                    var replier = await _unitOfWork.Users.GetByIdAsync(dto.UserId);

                    await _notiService.CreateAsync(
                        parentComment.UserId, // Gửi cho chủ comment cha
                        $"{replier.Username} đã trả lời bình luận của bạn.",
                        $"/comic/{dto.ComicId}#comment-{entity.Id}", // Link neo tới comment
                        NotificationType.Interaction
                    );
                }
            }

                _cache.Remove($"comments_comic_{dto.ComicId}");

            return _mapper.Map<CommentDTO>(entity);
        }

        public override async Task<CommentDTO> UpdateAsync(int id, CommentUpdateDTO dto)
        {
            var entity = await _commentRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Comment not found");

            int comicId = entity.ComicId;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _commentRepo.UpdateAsync(entity);
            await _unitOfWork.CommitAsync(); 

            _cache.Remove($"comments_comic_{comicId}");

            return _mapper.Map<CommentDTO>(entity);
        }

        public override async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
            var entity = await _commentRepo.GetByIdAsync(id);

            var result = await base.DeleteAsync(id, hardDelete); 

            if (result && entity != null)
            {
                _cache.Remove($"comments_comic_{entity.ComicId}");
            }
            return result;
        }

        // --- READ (Custom Method) ---

        public async Task<List<CommentDTO>> GetByComicAsync(int comicId)
        {
            string key = $"comments_comic_{comicId}";

            var comments = await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
              
                return await _commentRepo.GetByComicAsync(comicId);
            });

            return _mapper.Map<List<CommentDTO>>(comments);
        }
    }
}