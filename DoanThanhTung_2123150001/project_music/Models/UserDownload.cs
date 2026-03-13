using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class UserDownload
{
    public string DownloadId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string SongId { get; set; } = null!;

    public string DeviceId { get; set; } = null!;

    public DateTime? DownloadedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual Song Song { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
