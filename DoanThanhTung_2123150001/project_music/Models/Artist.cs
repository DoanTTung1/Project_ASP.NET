using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Artist
{
    public string ArtistId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual ICollection<ArtistSong> ArtistSongs { get; set; } = new List<ArtistSong>();

    public virtual ICollection<UserFollowArtist> UserFollowArtists { get; set; } = new List<UserFollowArtist>();
}
