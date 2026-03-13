using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Episode
{
    public string EpisodeId { get; set; } = null!;

    public string PodcastId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationSeconds { get; set; }

    public string AudioUrl { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public virtual Podcast Podcast { get; set; } = null!;
}
