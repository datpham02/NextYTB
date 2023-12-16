

namespace NextYTB.DTO
{
    public class ChannelInfoDTO
    {
        public int Id { get; set; }
        public string Avatar { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int SubcriberCount { get; set; } = 0;
        public int VideoCount { get; set; } = 0;


    }
}
