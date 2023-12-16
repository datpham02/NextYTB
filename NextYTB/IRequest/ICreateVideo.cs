namespace NextYTB.IRequest
{
    public class ICreateVideo
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int ChannelOwnerId { get; set; }

        public string Src { get; set; } = null!;

        public string Poster { get; set; } = null!;

        public string Duration { get; set; }
    }
}
