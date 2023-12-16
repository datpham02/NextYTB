using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public virtual Channel? Channel { get; set; }

    public virtual ICollection<Comment> CommentAccountOwners { get; set; } = new List<Comment>();

    public virtual ICollection<Comment> CommentReplyToAccounts { get; set; } = new List<Comment>();

    public virtual ICollection<Subcriber> Subcribers { get; set; } = new List<Subcriber>();

    public virtual ICollection<UserDisLikeComment> UserDisLikeComments { get; set; } = new List<UserDisLikeComment>();

    public virtual ICollection<UserDisLikeVideo> UserDisLikeVideos { get; set; } = new List<UserDisLikeVideo>();

    public virtual ICollection<UserLikeComment> UserLikeComments { get; set; } = new List<UserLikeComment>();

    public virtual ICollection<UserLikeVideo> UserLikeVideos { get; set; } = new List<UserLikeVideo>();

    public virtual ICollection<UserWatchLateVideo> UserWatchLateVideos { get; set; } = new List<UserWatchLateVideo>();

    public virtual ICollection<UserWatchedVideo> UserWatchedVideos { get; set; } = new List<UserWatchedVideo>();
}
