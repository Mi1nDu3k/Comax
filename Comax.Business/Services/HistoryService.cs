using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.History;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<HistoryDTO>> GetHistoryAsync(int userId)
        {
            var entities = await _unitOfWork.Histories.GetByUserAsync(userId);
            return _mapper.Map<List<HistoryDTO>>(entities);
        }

        public async Task AddOrUpdateHistoryAsync(int userId, HistoryCreateDTO dto)
        {
            if (userId <= 0) throw new Exception("User ID Invalid");


            var existing = await _unitOfWork.Histories.GetByUserAndComicAsync(userId, dto.ComicId);

            if (existing != null)
            {

                existing.ChapterId = dto.ChapterId;
                existing.LastReadTime = DateTime.UtcNow;


                ((Comax.Data.Repositories.HistoryRepository)_unitOfWork.Histories).ForceUpdate(existing);
            }
            else
            {

                var newHistory = new History
                {
                    UserId = userId,
                    ComicId = dto.ComicId,
                    ChapterId = dto.ChapterId,
                    LastReadTime = DateTime.UtcNow
                };
                await _unitOfWork.Histories.AddAsync(newHistory);
            }


            var rows = await _unitOfWork.CommitAsync();


            if (rows == 0) throw new Exception("DB Error: SaveChanges trả về 0 dòng!");
        }

        public async Task DeleteHistoryAsync(int userId, int id)
        {
            var history = await _unitOfWork.Histories.GetByIdAsync(id);
            if (history != null && history.UserId == userId)
            {
                await _unitOfWork.Histories.DeleteAsync(id);
                await _unitOfWork.CommitAsync();
            }
        }
        public async Task DeleteAllHistoryAsync(int userId)
        {
            
            var histories = await _unitOfWork.Histories.GetByUserAsync(userId);

            if (histories != null && histories.Any())
            {
               
                foreach (var item in histories)
                {
                 
                    await _unitOfWork.Histories.DeleteAsync(item.Id);
                }
                await _unitOfWork.CommitAsync();
            }
        }
    }
}