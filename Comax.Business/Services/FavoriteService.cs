using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int comicId)
        {
            var existing = await _unitOfWork.Favorites.GetAsync(userId, comicId);
            if (existing != null)
            {
                await _unitOfWork.Favorites.RemoveAsync(existing);
                await _unitOfWork.CommitAsync();
                return false; // Removed
            }

            var favorite = new Favorite { UserId = userId, ComicId = comicId };
            await _unitOfWork.Favorites.AddAsync(favorite);
            await _unitOfWork.CommitAsync();
            return true; // Added
        }

        public async Task<List<ComicDTO>> GetUserFavoritesAsync(int userId)
        {
            var comics = await _unitOfWork.Favorites.GetUserFavoritesAsync(userId);
            return _mapper.Map<List<ComicDTO>>(comics);
        }

        public async Task<bool> IsFavoritedAsync(int userId, int comicId)
        {
            var existing = await _unitOfWork.Favorites.GetAsync(userId, comicId);
            return existing != null;
        }

        public async Task UnfavoriteAsync(int userId, int comicId)
        {
            var existing = await _unitOfWork.Favorites.GetAsync(userId, comicId);
            if (existing != null)
            {
                await _unitOfWork.Favorites.RemoveAsync(existing);
                await _unitOfWork.CommitAsync();
            }

        }
        public async Task<List<ComicDTO>> GetFavoritesByUserIdAsync(int userId)
        {
            var comics = await _unitOfWork.Favorites.GetUserFavoritesAsync(userId);

            return _mapper.Map<List<ComicDTO>>(comics);
        }
        }
    }
