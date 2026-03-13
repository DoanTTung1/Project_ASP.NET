using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Song
{
    public string SongId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? AlbumId { get; set; }

    public int DurationSeconds { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public long? TotalPlays { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Album? Album { get; set; }

    public virtual ICollection<ArtistSong> ArtistSongs { get; set; } = new List<ArtistSong>();

    public virtual ICollection<AudioFile> AudioFiles { get; set; } = new List<AudioFile>();

    public virtual ICollection<PlayHistory> PlayHistories { get; set; } = new List<PlayHistory>();

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual ICollection<SongLyric> SongLyrics { get; set; } = new List<SongLyric>();

    public virtual ICollection<UserDownload> UserDownloads { get; set; } = new List<UserDownload>();

    public virtual ICollection<UserFavoriteSong> UserFavoriteSongs { get; set; } = new List<UserFavoriteSong>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
