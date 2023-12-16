namespace NextYTB.DTO
{
    public class AccountDTO
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public virtual ChannelDTO Channel { get; set; }
        public virtual ICollection<SubcriberDTO> Subcribers { get; set; } = new List<SubcriberDTO>();
    }
}
