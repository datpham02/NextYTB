

namespace NextYTB.DTO
{
    public class UserVideoWatchedDTO
    {
        public int Id { get; set; }
    
        public DateTime? WatchedAt { get; set; }

        public virtual VideoDTO Video { get; set; } = null!;
    }
}
