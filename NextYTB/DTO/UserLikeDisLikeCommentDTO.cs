

namespace NextYTB.DTO
{
    public class UserLikeDisLikeCommentDTO
    {
        public DateTime? CreateAt { get; set; }

        public virtual AccountInfoDTO Account { get; set; } = null!;
    }
}
