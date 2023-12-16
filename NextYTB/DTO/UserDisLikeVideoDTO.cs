

namespace NextYTB.DTO
{
    public class UserDisLikeVideoDTO
    {
        public int Id { get; set; }

   

        public DateTime? DisLikeAt { get; set; }

        public virtual VideoDTO Video { get; set; } = null!;


    }
}
