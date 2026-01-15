using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly ComaxDbContext _context;
        public FavoriteRepository(ComaxDbContext context) { _context = context; }

        public async Task AddAsync(Favorite favorite) => await _context.Favorites.AddAsync(favorite);
        public Task RemoveAsync(Favorite favorite) { _context.Favorites.Remove(favorite); return Task.CompletedTask; }

        public async Task<Favorite?> GetAsync(int userId, int comicId)
            => await _context.Favorites.FindAsync(userId, comicId);

        public async Task<List<Comic>> GetUserFavoritesAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Comic) 
                .Select(f => f.Comic)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserIdsByComicIdAsync(int comicId)
        {
            return await _context.Favorites
                .Where(f => f.ComicId == comicId)
                .Select(f => f.UserId)
                .ToListAsync();
        }
        public async Task<List<int>> GetUserIdsByComicIdPagedAsync(int comicId, int lastUserId, int take)
        {
            
            return await _context.Favorites
                .Where(f => f.ComicId == comicId && f.UserId > lastUserId) 
                .Take(take)
                .Select(f => f.UserId)
                .ToListAsync();
        }
    }
}