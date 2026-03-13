using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class AudioFile
{
    public string FileId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public string Quality { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public long SizeBytes { get; set; }

    public virtual Song Song { get; set; } = null!;
}
