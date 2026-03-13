using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class UserFollowUser
{
    public string FollowerId { get; set; } = null!;

    public string FollowingId { get; set; } = null!;

    public DateTime? FollowedAt { get; set; }

    public virtual User Follower { get; set; } = null!;

    public virtual User Following { get; set; } = null!;
}
