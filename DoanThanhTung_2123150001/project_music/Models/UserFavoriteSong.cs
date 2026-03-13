using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class UserFavoriteSong
{
    public string UserId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public DateTime? LikedAt { get; set; }

    public virtual Song Song { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
