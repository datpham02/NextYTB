using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class UserDisLikeComment
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int CommentId { get; set; }

    public DateTime? DisLikeAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Comment Comment { get; set; } = null!;
}
