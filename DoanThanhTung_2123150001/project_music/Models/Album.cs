using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Album
{
    public string AlbumId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public string ArtistId { get; set; } = null!;

    public string? CoverUrl { get; set; }

    public string AlbumType { get; set; } = null!;

    public virtual Artist Artist { get; set; } = null!;

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
