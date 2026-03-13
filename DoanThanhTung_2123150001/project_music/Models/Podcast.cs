using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Podcast
{
    public string PodcastId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string HostName { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
