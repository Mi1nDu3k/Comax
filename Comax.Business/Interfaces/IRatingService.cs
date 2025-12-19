using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Comax.Business.Interfaces
{
    public interface IRatingRepository : IBaseRepository<Rating>
    {
        Task<List<Rating>> GetByComicAsync(int comicId);
        Task<double> GetAverageScoreAsync(int comicId);
    }
}
