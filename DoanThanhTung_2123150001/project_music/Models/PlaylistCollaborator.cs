using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class PlaylistCollaborator
{
    public string PlaylistId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? Role { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
