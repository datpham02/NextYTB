namespace NextYTB.DTO
{
    public class AccountInfoDTO
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public virtual ChannelDTO Channel { get; set; }
    }
}
