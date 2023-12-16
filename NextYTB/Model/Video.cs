using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class Video
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public string Duration { get; set; } = null!;

    public int ChannelOwnerId { get; set; }

    public string Src { get; set; } = null!;

    public string Poster { get; set; } = null!;

    public virtual Channel ChannelOwner { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<UserDisLikeVideo> UserDisLikeVideos { get; set; } = new List<UserDisLikeVideo>();

    public virtual ICollection<UserLikeVideo> UserLikeVideos { get; set; } = new List<UserLikeVideo>();

    public virtual ICollection<UserWatchLateVideo> UserWatchLateVideos { get; set; } = new List<UserWatchLateVideo>();

    public virtual ICollection<UserWatchedVideo> UserWatchedVideos { get; set; } = new List<UserWatchedVideo>();
}
