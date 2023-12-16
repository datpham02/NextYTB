using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class UserLikeVideo
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int VideoId { get; set; }

    public DateTime? LikeAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Video Video { get; set; } = null!;
}
