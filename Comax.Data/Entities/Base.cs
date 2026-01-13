using System.ComponentModel.DataAnnotations;

namespace Comax.Data.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public int ViewCount { get; set; } = 0;
    }
}