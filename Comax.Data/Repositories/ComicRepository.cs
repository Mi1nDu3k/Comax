using Comax.Common.DTOs.Pagination;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ComicRepository : BaseRepository<Comic>, IComicRepository
    {
        public ComicRepository(ComaxDbContext context) : base(context) { }

        // SỬA: Chỉnh lại lỗi gõ nhầm 'cg.ge' thành 'cc.Category'
        public async Task<Comic?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Author)
                .Include(c => c.ComicCategories)
                .ThenInclude(cc => cc.Category) 
                .FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task<Comic?> GetBySlugAsync(string slug)
        {
            return await _context.Comics
                .Include(c => c.Author)
          
                .Include(c => c.ComicCategories)
                .ThenInclude(cc => cc.Category)
         
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<IEnumerable<Comic>> SearchByTitleAsync(string title)
        {
            return await _dbSet
                .Where(c => c.Title.Contains(title))
                .ToListAsync();
        }
        public async Task<List<Comic>> SearchAsync(string keyword, int limit = 0)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<Comic>();

        
            IQueryable<Comic> query = _dbSet;


            query = query
                .Include(c => c.Chapters) 
                .Where(c => c.Title.Contains(keyword) || (c.Author != null && c.Author.Name.Contains(keyword)))
                .OrderByDescending(c => c.ViewCount)
                .AsNoTracking();

         
            if (limit > 0)
            {
           
                query = query.Take(limit);
            }
            else
            {
                query = query.Take(50); 
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId)
        {
            return await _dbSet
                .Where(c => c.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Where(c => c.ComicCategories.Any(cc => cc.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<PagedList<Comic>> GetLatestUpdatedComicsAsync(PaginationParams param)
        {
            var query = _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                .ThenInclude(cc => cc.Category)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Chapters.Max(ch => (DateTime?)ch.PublishDate) ?? c.CreatedAt);

            var count = await query.CountAsync();
            var items = await query
                .Skip((param.PageNumber - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new PagedList<Comic>(items, count, param.PageNumber, param.PageSize);
        }

        public async Task<Comic?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }


        public async Task<PagedList<Comic>> GetTrashAsync(PaginationParams param, string searchTerm)
        {
            var query = _context.Comics.IgnoreQueryFilters()
                                        .Where(c => c.IsDeleted == true)
                                        .Include(c => c.ComicCategories) // Lưu ý: Nếu muốn hiện tên thể loại trong thùng rác thì cũng cần ThenInclude ở đây
                                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm));
            }

            query = query.OrderByDescending(c => c.UpdatedAt);

            var count = await query.CountAsync();
            var items = await query
                .Skip((param.PageNumber - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new PagedList<Comic>(items, count, param.PageNumber, param.PageSize);
        }

        public async Task<Comic?> GetDeletedByIdAsync(int id)
        {
            return await _context.Comics.IgnoreQueryFilters()
                                        .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == true);
        }

        public void HardDelete(Comic comic)
        {
            _context.Comics.Remove(comic);
        }
        public async Task<int> DeleteComicsInTrashOlderThanAsync(DateTime thresholdDate)
        {
            
            return await _context.Comics
                .IgnoreQueryFilters() 
                .Where(c => c.IsDeleted && c.DeletedAt <= thresholdDate)
                .ExecuteDeleteAsync();
        }
        public async Task<List<Comic>> GetRelatedComicsAsync(int comicId, int limit = 6)
        {
            //Lấy thể loại truyện hiện tại
            var currentComic = await _dbSet
                .Include(c => c.ComicCategories)
                .FirstOrDefaultAsync(c => c.Id == comicId);

            if (currentComic == null) return new List<Comic>();

            // Lấy danh sách ID thể loại của truyện này
            var categoryIds = currentComic.ComicCategories.Select(cc => cc.CategoryId).ToList();
            var authorId = currentComic.AuthorId;

            // B. Truy vấn tìm truyện tương tự
            var query = _dbSet.AsNoTracking()
                .Where(c => c.Id != comicId && !c.IsDeleted) 
                .Where(c => c.ComicCategories.Any(cc => categoryIds.Contains(cc.CategoryId)) 
                         || (c.AuthorId != null && c.AuthorId == authorId)); 

            // C. (AI Logic đơn giản) Sắp xếp theo độ ưu tiên:
            // 1. Cùng tác giả (Ưu tiên cao nhất)
            // 2. Nhiều lượt xem (Phổ biến)
            var result = await query
                .OrderByDescending(c => c.AuthorId == authorId) 
                .ThenByDescending(c => c.ViewCount) 
                .Take(limit)
                .Include(c => c.ComicCategories).ThenInclude(cc => cc.Category) 
                .ToListAsync();

            return result;
        }
    }
}