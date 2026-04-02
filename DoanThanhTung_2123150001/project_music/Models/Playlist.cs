using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Playlist
{
    public string PlaylistId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    public bool? IsPublic { get; set; }

    public DateTime? CreatedAt { get; set; }
    public bool? IsDeleted { get; set; }

    public virtual ICollection<PlaylistCollaborator> PlaylistCollaborators { get; set; } = new List<PlaylistCollaborator>();

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual User User { get; set; } = null!;
}
