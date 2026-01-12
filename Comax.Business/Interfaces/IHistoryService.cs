using System.Collections.Generic;
using System.Threading.Tasks;
using Comax.Common.DTOs.History; 

namespace Comax.Business.Services.Interfaces
{
    public interface IHistoryService
    {
 
        Task<List<HistoryDTO>> GetHistoryAsync(int userId);

        Task AddOrUpdateHistoryAsync(int userId, HistoryCreateDTO dto);

        Task DeleteHistoryAsync(int userId, int id);
        Task DeleteAllHistoryAsync(int userId);
    }
}