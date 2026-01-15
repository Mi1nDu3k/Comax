using Comax.Data.Entities;
using System;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IAuthorRepository Authors { get; }
        ICategoryRepository Categories { get; }
        IComicRepository Comics { get; }
        IChapterRepository Chapters { get; }
        ICommentRepository Comments { get; }
        IRatingRepository Ratings { get; }
        IFavoriteRepository Favorites { get; }
        INotificationRepository Notifications { get; }
        IHistoryRepository Histories { get; }
        Task<int> CommitAsync();
        void ClearChangeTracker();
    }
}