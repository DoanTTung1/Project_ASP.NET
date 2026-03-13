using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class UserFollowArtist
{
    public string UserId { get; set; } = null!;

    public string ArtistId { get; set; } = null!;

    public DateTime? FollowedAt { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
