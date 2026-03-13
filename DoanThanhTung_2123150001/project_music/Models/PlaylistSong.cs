using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class PlaylistSong
{
    public string PlaylistId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public DateTime? AddedAt { get; set; }

    public int PositionOrder { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
