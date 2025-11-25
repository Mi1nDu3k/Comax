using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repo;
        public RatingService(IRatingRepository repo) { _repo = repo; }

        public async Task<Rating> CreateAsync(RatingCreateDTO dto)
        {
            var rating = new Rating
            {
                Score = dto.Score,
                Comment = dto.Comment,
                ComicId = dto.ComicId,
                UserId = dto.UserId
            };
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

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<List<Rating>> GetByComicAsync(int comicId) => await _repo.GetByComicAsync(comicId);

        public async Task<double> GetAverageScoreAsync(int comicId) => await _repo.GetAverageScoreAsync(comicId);
    }
}
