

namespace NextYTB.DTO
{
    public class UserVideoWatchLateDTO
    {
        public int Id { get; set; }

        public virtual VideoDTO Video { get; set; } = null!;
    }
}
