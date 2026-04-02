using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace project_music.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public bool? IsPremium { get; set; }

    public DateTime? PremiumExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Column("role")]
    public string? Role { get; set; }

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("zalo_id")]
    public string? ZaloId { get; set; }

    public bool IsDeleted { get; set; }
   public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<PlayHistory> PlayHistories { get; set; } = new List<PlayHistory>();

    public virtual ICollection<PlaylistCollaborator> PlaylistCollaborators { get; set; } = new List<PlaylistCollaborator>();

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserDownload> UserDownloads { get; set; } = new List<UserDownload>();

    public virtual ICollection<UserFavoriteSong> UserFavoriteSongs { get; set; } = new List<UserFavoriteSong>();

    public virtual ICollection<UserFollowArtist> UserFollowArtists { get; set; } = new List<UserFollowArtist>();

    public virtual ICollection<UserFollowUser> UserFollowUserFollowers { get; set; } = new List<UserFollowUser>();

    public virtual ICollection<UserFollowUser> UserFollowUserFollowings { get; set; } = new List<UserFollowUser>();
}

