using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Comax.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface IRatingService
    {
        Task<Rating> CreateAsync(RatingCreateDTO dto);
        Task<Rating?> UpdateAsync(int id, RatingUpdateDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<List<Rating>> GetByComicAsync(int comicId);
        Task<double> GetAverageScoreAsync(int comicId);
    }
}
