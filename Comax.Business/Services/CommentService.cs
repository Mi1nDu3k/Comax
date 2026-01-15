using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs.Comment;
using Comax.Common.Enums;
using Comax.Data.Entities;
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
            _notiService = notiService;
        }

        public async Task<List<CommentDTO>> GetParentsByComicAsync(int comicId, int page = 1)
        {
            int pageSize = 5;
            var entities = await _commentRepo.GetParentsByComicAsync(comicId, page, pageSize);
            var dtos = _mapper.Map<List<CommentDTO>>(entities);

            for (int i = 0; i < dtos.Count; i++)
            {
                dtos[i].ReplyCount = entities[i].Replies?.Count ?? 0;
                dtos[i].Replies = new List<CommentDTO>();
            }
            return dtos;
        }

        public async Task<List<CommentDTO>> GetRepliesAsync(int parentId, int page = 1)
        {
            int pageSize = 5;
            var entities = await _commentRepo.GetRepliesAsync(parentId, page, pageSize);
            return _mapper.Map<List<CommentDTO>>(entities);
        }


        public override async Task<CommentDTO> CreateAsync(CommentCreateDTO dto)
        {
            var entity = _mapper.Map<Comment>(dto);
            entity.CreatedAt = DateTime.UtcNow;


            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user != null) entity.User = user;

            await _commentRepo.AddAsync(entity);
            await _unitOfWork.CommitAsync();


            if (dto.ParentId.HasValue)
            {
                var parentComment = await _commentRepo.GetByIdAsync(dto.ParentId.Value);


                if (parentComment != null && parentComment.UserId != dto.UserId)

                {
                    if (user != null)
                    {
                        string replierName = user.Username;
                        string notificationUrl = $"/truyen/{dto.ComicId}?commentId={entity.Id}";
                        await _notiService.CreateAndSendNotificationAsync(
                         parentComment.UserId, // Tham số 1: UserId
                        string.Format(SystemMessages.Notification.CommentReply, replierName),
                         notificationUrl // Tham số 3: Url
 );
                    }
                }
            }

            _cache.Remove($"comments_comic_{dto.ComicId}");

            return _mapper.Map<CommentDTO>(entity);
        }

        public override async Task<CommentDTO> UpdateAsync(int id, CommentUpdateDTO dto)
        {
            var entity = await _commentRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception(SystemMessages.Comment.NotFound);

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
            if (entity == null) return false;

            var comicId = entity.ComicId;
            var result = await base.DeleteAsync(id, hardDelete);

            if (result) _cache.Remove($"comments_comic_{comicId}");
            return result;
        }

        public async Task<List<CommentDTO>> GetByComicAsync(int comicId)
        {
            string key = $"comments_comic_{comicId}";
            var comments = await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _commentRepo.GetParentsByComicAsync(comicId, 1, 1000);
            });
            return _mapper.Map<List<CommentDTO>>(comments);
        }
    }
}