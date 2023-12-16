using System;
using System.Collections.Generic;

namespace NextYTB.Model;

public partial class Channel
{
    public int Id { get; set; }

    public string Avatar { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int AccountOwnerId { get; set; }

    public virtual Account AccountOwner { get; set; } = null!;

    public virtual ICollection<Subcriber> Subcribers { get; set; } = new List<Subcriber>();

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
