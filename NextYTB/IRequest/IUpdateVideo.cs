namespace NextYTB.IRequest
{
    public class IUpdateVideo
    {
        public string? Title { get; set; } = null!;

        public string? Description { get; set; }

        public int VideoId { get; set; }

        public string? Poster { get; set; } = null!;

    }
}
