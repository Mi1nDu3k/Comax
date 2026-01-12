using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore; // Cần thiết cho các hàm Async
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComicRepository _comicRepo;

        public RatingService(
            IRatingRepository repo,
            IUnitOfWork unitOfWork,
            IComicRepository comicRepo)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _comicRepo = comicRepo;
        }

        // --- CÁC HÀM CŨ ---
        public async Task<Rating> CreateAsync(RatingCreateDTO dto)
        {
            var rating = new Rating { Score = dto.Score, Comment = dto.Comment, ComicId = dto.ComicId, UserId = dto.UserId };
            return await _repo.AddAsync(rating);
        }

        public async Task<Rating?> UpdateAsync(int id, RatingUpdateDTO dto)
        {
            var rating = await _repo.GetByIdAsync(id);
            if (rating == null) return null;
            rating.Score = dto.Score;
            rating.Comment = dto.Comment;
            rating.UpdatedAt = DateTime.UtcNow;
            return await _repo.UpdateAsync(rating);
        }

        public async Task<bool> DeleteAsync(int id, bool hardDelete = false)
            => await _repo.DeleteAsync(id, hardDelete);

        public async Task<List<Rating>> GetByComicAsync(int comicId) => await _repo.GetByComicAsync(comicId);

        public async Task<double> GetAverageScoreAsync(int comicId) => await _repo.GetAverageScoreAsync(comicId);

        // --- CÁC HÀM MỚI ĐÃ SỬA LỖI ---

        public async Task<double> AddOrUpdateRatingAsync(int userId, RatingCreateDTO dto)
        {
            
            var existingRating = await _repo.GetByUserAndComicAsync(userId, dto.ComicId);

            if (existingRating != null)
            {
                // UPDATE: Cập nhật điểm
                existingRating.Score = dto.Score;
                existingRating.UpdatedAt = DateTime.UtcNow;

                // Sửa lỗi: Dùng UpdateAsync của Repo thay vì Update của DbSet
                await _repo.UpdateAsync(existingRating);
            }
            else
            {
                // CREATE: Tạo mới
                var newRating = new Rating
                {
                    UserId = userId,
                    ComicId = dto.ComicId,
                    Score = dto.Score,
                    CreatedAt = DateTime.UtcNow
                };
                await _repo.AddAsync(newRating);
            }

            // Lưu thay đổi
            await _unitOfWork.CommitAsync();

            // 2. Tính lại điểm trung bình cho Comic
            await RecalculateComicRating(dto.ComicId);

            // 3. Trả về kết quả mới
            return await GetAverageScoreAsync(dto.ComicId);
        }

        public async Task<int> GetUserRatingForComicAsync(int userId, int comicId)
        {
            // Dùng hàm mới trong Repository
            var rating = await _repo.GetByUserAndComicAsync(userId, comicId);
            return rating?.Score ?? 0;
        }

        private async Task RecalculateComicRating(int comicId)
        {
            // Lấy danh sách rating để tính trung bình
            var ratings = await _repo.GetByComicAsync(comicId);

            if (ratings.Any())
            {
                var average = ratings.Average(r => r.Score);

                // Cập nhật vào bảng Comic
                var comic = await _comicRepo.GetByIdAsync(comicId);
                if (comic != null)
                {
                    comic.Rating = (float)Math.Round(average, 1);
                    await _comicRepo.UpdateAsync(comic);
                    await _unitOfWork.CommitAsync();
                }
            }
        }
    }
}