namespace NextYTB.DTO
{
    public class WatchedVideoDTO
    {
        public int Id { get; set; }
        public DateTime? WatchedAt { get; set; } = DateTime.Now;
        public VideoDTO Video { get; set; } = null!;
    }
}
