using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class ArtistSong
{
    public string ArtistId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public string? Role { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
