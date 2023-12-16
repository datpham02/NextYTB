using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class Subcriber
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int ChannelId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Channel Channel { get; set; } = null!;
}
