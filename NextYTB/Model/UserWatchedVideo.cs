using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class UserWatchedVideo
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int VideoId { get; set; }

    public DateTime? WatchedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Video Video { get; set; } = null!;
}
