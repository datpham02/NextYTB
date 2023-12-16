namespace NextYTB.DTO
{
    public class SubcriberDTO
    {
        public int Id { get; set; }

        public virtual ChannelDTO Channel { get; set; } = null!;

       
    }
}
