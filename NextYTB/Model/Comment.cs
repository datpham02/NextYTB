using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreateAt { get; set; }

    public int AccountOwnerId { get; set; }

    public int VideoId { get; set; }

    public int? ParentCommentId { get; set; }

    public int? ReplyToAccountId { get; set; }

    public virtual Account AccountOwner { get; set; } = null!;

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual Account? ReplyToAccount { get; set; }

    public virtual ICollection<UserDisLikeComment> UserDisLikeComments { get; set; } = new List<UserDisLikeComment>();

    public virtual ICollection<UserLikeComment> UserLikeComments { get; set; } = new List<UserLikeComment>();

    public virtual Video Video { get; set; } = null!;
}
