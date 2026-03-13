using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class PlayHistory
{
    public string HistoryId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public DateTime? PlayedAt { get; set; }

    public int ListenDurationSeconds { get; set; }

    public virtual Song Song { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
