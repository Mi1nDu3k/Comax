using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ComaxDbContext _context;

        private IUserRepository _users;
        private IRoleRepository _roles;
        private IAuthorRepository _authors;
        private ICategoryRepository _categories;
        private IComicRepository _comics;
        private IChapterRepository _chapters;
        private ICommentRepository _comments;
        private IRatingRepository _ratings;
        private INotificationRepository _notifications;
        private IFavoriteRepository _favorites;
        private IHistoryRepository _histories;

        public UnitOfWork(ComaxDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IAuthorRepository Authors => _authors ??= new AuthorRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IComicRepository Comics => _comics ??= new ComicRepository(_context);
        public IChapterRepository Chapters => _chapters ??= new ChapterRepository(_context);
        public ICommentRepository Comments => _comments ??= new CommentRepository(_context);
        public IRatingRepository Ratings => _ratings ??= new RatingRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);
        public IHistoryRepository Histories => _histories ??= new HistoryRepository(_context);
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }
    }
}