using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class SongLyric
{
    public string LyricId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public string? Language { get; set; }

    public string? SyncType { get; set; }

    public string Content { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
