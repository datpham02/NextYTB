

namespace NextYTB.DTO
{
    public class UserLikeVideoDTO
    {
        public int Id { get; set; }

        public DateTime? LikeAt { get; set; }

        public virtual VideoDTO Video { get; set; } = null!;
    }
}
