using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class UserDisLikeVideo
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int VideoId { get; set; }

    public DateTime? DisLikeAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Video Video { get; set; } = null!;
}
