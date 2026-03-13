using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace project_music.Models;

public partial class MusicDbContext : DbContext
{
    public MusicDbContext()
    {
    }

    public MusicDbContext(DbContextOptions<MusicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Album> Albums { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ArtistSong> ArtistSongs { get; set; }

    public virtual DbSet<AudioFile> AudioFiles { get; set; }

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<PlayHistory> PlayHistories { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistCollaborator> PlaylistCollaborators { get; set; }

    public virtual DbSet<PlaylistSong> PlaylistSongs { get; set; }

    public virtual DbSet<Podcast> Podcasts { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<SongLyric> SongLyrics { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDownload> UserDownloads { get; set; }

    public virtual DbSet<UserFavoriteSong> UserFavoriteSongs { get; set; }

    public virtual DbSet<UserFollowArtist> UserFollowArtists { get; set; }

    public virtual DbSet<UserFollowUser> UserFollowUsers { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.AlbumId).HasName("PRIMARY");

            entity.ToTable("albums");

            entity.HasIndex(e => e.ArtistId, "artist_id");

            entity.Property(e => e.AlbumId)
                .HasMaxLength(36)
                .HasColumnName("album_id");
            entity.Property(e => e.AlbumType)
                .HasColumnType("enum('SINGLE','EP','ALBUM')")
                .HasColumnName("album_type");
            entity.Property(e => e.ArtistId)
                .HasMaxLength(36)
                .HasColumnName("artist_id");
            entity.Property(e => e.CoverUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_url");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Artist).WithMany(p => p.Albums)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("albums_ibfk_1");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.ArtistId).HasName("PRIMARY");

            entity.ToTable("artists");

            entity.Property(e => e.ArtistId)
                .HasMaxLength(36)
                .HasColumnName("artist_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Bio)
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.CoverUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ArtistSong>(entity =>
        {
            entity.HasKey(e => new { e.ArtistId, e.SongId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("artist_song");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.Property(e => e.ArtistId)
                .HasMaxLength(36)
                .HasColumnName("artist_id");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'MAIN'")
                .HasColumnType("enum('MAIN','FEATURED','PRODUCER')")
                .HasColumnName("role");

            entity.HasOne(d => d.Artist).WithMany(p => p.ArtistSongs)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artist_song_ibfk_1");

            entity.HasOne(d => d.Song).WithMany(p => p.ArtistSongs)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artist_song_ibfk_2");
        });

        modelBuilder.Entity<AudioFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PRIMARY");

            entity.ToTable("audio_files");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.Property(e => e.FileId)
                .HasMaxLength(36)
                .HasColumnName("file_id");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(255)
                .HasColumnName("file_url");
            entity.Property(e => e.Quality)
                .HasColumnType("enum('128KBPS','320KBPS','LOSSLESS')")
                .HasColumnName("quality");
            entity.Property(e => e.SizeBytes).HasColumnName("size_bytes");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");

            entity.HasOne(d => d.Song).WithMany(p => p.AudioFiles)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("audio_files_ibfk_1");
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasKey(e => e.EpisodeId).HasName("PRIMARY");

            entity.ToTable("episodes");

            entity.HasIndex(e => e.PodcastId, "podcast_id");

            entity.Property(e => e.EpisodeId)
                .HasMaxLength(36)
                .HasColumnName("episode_id");
            entity.Property(e => e.AudioUrl)
                .HasMaxLength(255)
                .HasColumnName("audio_url");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.PodcastId)
                .HasMaxLength(36)
                .HasColumnName("podcast_id");
            entity.Property(e => e.PublishedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("published_at");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Podcast).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.PodcastId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("episodes_ibfk_1");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PRIMARY");

            entity.ToTable("genres");

            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasMany(d => d.Songs).WithMany(p => p.Genres)
                .UsingEntity<Dictionary<string, object>>(
                    "GenreSong",
                    r => r.HasOne<Song>().WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("genre_song_ibfk_2"),
                    l => l.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("genre_song_ibfk_1"),
                    j =>
                    {
                        j.HasKey("GenreId", "SongId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("genre_song");
                        j.HasIndex(new[] { "SongId" }, "song_id");
                        j.IndexerProperty<int>("GenreId").HasColumnName("genre_id");
                        j.IndexerProperty<string>("SongId")
                            .HasMaxLength(36)
                            .HasColumnName("song_id");
                    });
        });

        modelBuilder.Entity<PlayHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PRIMARY");

            entity.ToTable("play_history");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.HistoryId)
                .HasMaxLength(36)
                .HasColumnName("history_id");
            entity.Property(e => e.ListenDurationSeconds).HasColumnName("listen_duration_seconds");
            entity.Property(e => e.PlayedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("played_at");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Song).WithMany(p => p.PlayHistories)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("play_history_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.PlayHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("play_history_ibfk_1");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId).HasName("PRIMARY");

            entity.ToTable("playlists");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.PlaylistId)
                .HasMaxLength(36)
                .HasColumnName("playlist_id");
            entity.Property(e => e.CoverUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_public");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlists_ibfk_1");
        });

        modelBuilder.Entity<PlaylistCollaborator>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.UserId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("playlist_collaborators");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.PlaylistId)
                .HasMaxLength(36)
                .HasColumnName("playlist_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'EDITOR'")
                .HasColumnType("enum('OWNER','EDITOR')")
                .HasColumnName("role");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistCollaborators)
                .HasForeignKey(d => d.PlaylistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlist_collaborators_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.PlaylistCollaborators)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlist_collaborators_ibfk_2");
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.SongId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("playlist_songs");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.Property(e => e.PlaylistId)
                .HasMaxLength(36)
                .HasColumnName("playlist_id");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("added_at");
            entity.Property(e => e.PositionOrder).HasColumnName("position_order");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.PlaylistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlist_songs_ibfk_1");

            entity.HasOne(d => d.Song).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlist_songs_ibfk_2");
        });

        modelBuilder.Entity<Podcast>(entity =>
        {
            entity.HasKey(e => e.PodcastId).HasName("PRIMARY");

            entity.ToTable("podcasts");

            entity.Property(e => e.PodcastId)
                .HasMaxLength(36)
                .HasColumnName("podcast_id");
            entity.Property(e => e.CoverUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_url");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.HostName)
                .HasMaxLength(100)
                .HasColumnName("host_name");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.SongId).HasName("PRIMARY");

            entity.ToTable("songs");

            entity.HasIndex(e => e.AlbumId, "album_id");

            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.AlbumId)
                .HasMaxLength(36)
                .HasColumnName("album_id");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.TotalPlays)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_plays");

            entity.HasOne(d => d.Album).WithMany(p => p.Songs)
                .HasForeignKey(d => d.AlbumId)
                .HasConstraintName("songs_ibfk_1");
        });

        modelBuilder.Entity<SongLyric>(entity =>
        {
            entity.HasKey(e => e.LyricId).HasName("PRIMARY");

            entity.ToTable("song_lyrics");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.Property(e => e.LyricId)
                .HasMaxLength(36)
                .HasColumnName("lyric_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .HasDefaultValueSql("'vi'")
                .HasColumnName("language");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.SyncType)
                .HasDefaultValueSql("'UNSYNCED'")
                .HasColumnType("enum('UNSYNCED','LINE_SYNCED','WORD_SYNCED')")
                .HasColumnName("sync_type");

            entity.HasOne(d => d.Song).WithMany(p => p.SongLyrics)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("song_lyrics_ibfk_1");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PRIMARY");

            entity.ToTable("subscription_plans");

            entity.Property(e => e.PlanId)
                .HasMaxLength(36)
                .HasColumnName("plan_id");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Features)
                .HasColumnType("json")
                .HasColumnName("features");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.PlanId, "plan_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.TransactionId)
                .HasMaxLength(36)
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.PaidAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentGateway)
                .HasColumnType("enum('MOMO','ZALOPAY','STRIPE','PAYPAL')")
                .HasColumnName("payment_gateway");
            entity.Property(e => e.PlanId)
                .HasMaxLength(36)
                .HasColumnName("plan_id");
            entity.Property(e => e.Status)
                .HasColumnType("enum('PENDING','SUCCESS','FAILED')")
                .HasColumnName("status");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsPremium)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_premium");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PremiumExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("premium_expiry_date");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserDownload>(entity =>
        {
            entity.HasKey(e => e.DownloadId).HasName("PRIMARY");

            entity.ToTable("user_downloads");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.DownloadId)
                .HasMaxLength(36)
                .HasColumnName("download_id");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(100)
                .HasColumnName("device_id");
            entity.Property(e => e.DownloadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("downloaded_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Song).WithMany(p => p.UserDownloads)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_downloads_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UserDownloads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_downloads_ibfk_1");
        });

        modelBuilder.Entity<UserFavoriteSong>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SongId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("user_favorite_songs");

            entity.HasIndex(e => e.SongId, "song_id");

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.SongId)
                .HasMaxLength(36)
                .HasColumnName("song_id");
            entity.Property(e => e.LikedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("liked_at");

            entity.HasOne(d => d.Song).WithMany(p => p.UserFavoriteSongs)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_favorite_songs_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavoriteSongs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_favorite_songs_ibfk_1");
        });

        modelBuilder.Entity<UserFollowArtist>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArtistId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("user_follow_artist");

            entity.HasIndex(e => e.ArtistId, "artist_id");

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.ArtistId)
                .HasMaxLength(36)
                .HasColumnName("artist_id");
            entity.Property(e => e.FollowedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("followed_at");

            entity.HasOne(d => d.Artist).WithMany(p => p.UserFollowArtists)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_follow_artist_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UserFollowArtists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_follow_artist_ibfk_1");
        });

        modelBuilder.Entity<UserFollowUser>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowingId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("user_follow_user");

            entity.HasIndex(e => e.FollowingId, "following_id");

            entity.Property(e => e.FollowerId)
                .HasMaxLength(36)
                .HasColumnName("follower_id");
            entity.Property(e => e.FollowingId)
                .HasMaxLength(36)
                .HasColumnName("following_id");
            entity.Property(e => e.FollowedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("followed_at");

            entity.HasOne(d => d.Follower).WithMany(p => p.UserFollowUserFollowers)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_follow_user_ibfk_1");

            entity.HasOne(d => d.Following).WithMany(p => p.UserFollowUserFollowings)
                .HasForeignKey(d => d.FollowingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_follow_user_ibfk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
