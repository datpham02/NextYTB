namespace NextYTB.IRequest
{
    public class ICreateComment
    {
        public string Content { get; set; } = null!;
        public int AccountOwnerId { get; set; } = 0;

        public int VideoId { get; set; } = 0;

        public int? ParentCommentId { get; set; } = null;

        public int? ReplyToAccountId { get; set; } = 0;

       
    }
}
