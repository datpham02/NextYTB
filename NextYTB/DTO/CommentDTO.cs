



namespace NextYTB.DTO
{
    public class CommentDTO
    {
        public int Id { get; set; }

        public string Content { get; set; } = null!;

        public DateTime? CreateAt { get; set; }

        public int AccountOwnerId { get; set; }
        public int? ReplyToAccountId { get; set; }
        public int VideoId { get; set; }

        public virtual AccountInfoDTO AccountOwner { get; set; } = null!;

        public virtual ICollection<CommentDTO> Reply { get; set; } = new List<CommentDTO>();

        public virtual AccountInfoDTO? ReplyToAccount { get; set; }

        public virtual ICollection<UserLikeDisLikeCommentDTO> Like { get; set; } = new List<UserLikeDisLikeCommentDTO>();

        public virtual ICollection<UserLikeDisLikeCommentDTO> DisLike { get; set; } = new List<UserLikeDisLikeCommentDTO>();


    }
}
