


namespace NextYTB.DTO
{
    public class VideoDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public DateTime? CreateAt { get; set; } 

        public string Src { get; set; } = null!;

        public string Poster { get; set; } = null!;

        public int View {  get; set; } = 0;
        public string Duration {  get; set; } = null!;
        public ChannelInfoDTO Channel { get; set; }
        public int Like { get; set; } =0;

        public int DisLike { get; set; } = 0;
    }
}
