using Microsoft.EntityFrameworkCore; // 1. THÊM DÒNG NÀY VÀO ĐỂ CHẠY ĐƯỢC TO LIST ASYNC
using project_music.DTOs.Playlists;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Playlists
{

    public class PlaylistService : IPlaylistService
    {
        private readonly MusicDbContext _context;
        public PlaylistService(MusicDbContext context)
        {
            _context = context;
        }


        public async Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request)
        {
            var newPlaylist = new Playlist
            {
                PlaylistId = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Playlists.Add(newPlaylist);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new PlaylistResponse
            {
                PlaylistId = newPlaylist.PlaylistId,
                UserId = newPlaylist.UserId,
                CoverUrl = newPlaylist.CoverUrl,
                Name = newPlaylist.Name,
                Description = newPlaylist.Description,
                IsPublic = newPlaylist.IsPublic ?? false,
                CreatedAt = newPlaylist.CreatedAt,
                CreatorName = user?.Username ?? "Unknown",
                TotalSongs = 0
            };
        }

        public async Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId)
        {
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId && p.IsDeleted == false)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    IsPublic = p.IsPublic ?? false,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count()
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return playlists;
        }



        public async Task<bool> AddSongToPlaylistAsync(string userId, string playlistId, string songId)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null || playlist.IsDeleted == true)

                throw new Exception("Playlist không tồn tại hoặc đã bị xóa");

            if (playlist.UserId != userId)

                throw new Exception("Bạn không có quyền thêm bài hát vào playlist này");

            var song = await _context.Songs.FindAsync(songId);
            if (song == null || song.IsDeleted == true)

                throw new Exception("Bài hát không tồn tại hoặc đã bị xóa");

            var exits = await _context.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (exits) throw new Exception("Bài hát này đã có sẵn trong Playlist");
            int maxPosition = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Select(ps => (int?)ps.PositionOrder)
                .MaxAsync() ?? 0;

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                PositionOrder = maxPosition + 1
            };
            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePlaylistAsync(string userId, string playlistId, string name, string description)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId && p.IsDeleted == false);
            if (playlist == null) throw new Exception("Không tìm thấy Playlist hoặc bạn không có quyền sửa!");

            playlist.Name = name;
            playlist.Description = description;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlaylistAsync(string userId, string playlistId)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId && p.IsDeleted == false);
            if (playlist == null) throw new Exception("Không tìm thấy Playlist hoặc bạn không có quyền xóa!");

            // Xóa mềm
            playlist.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }


        // --- HÀM 1: LẤY CHI TIẾT PLAYLIST VÀ DANH SÁCH BÀI HÁT ---
        public async Task<PlaylistDetailResponse> GetPlaylistByIdAsync(string playlistId, string? currentUserId)
        {
            var playlist = await _context.Playlists
                .Where(p => p.PlaylistId == playlistId && p.IsDeleted == false)
                .Select(p => new PlaylistDetailResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    IsPublic = p.IsPublic ?? false,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count(),

                    // Lấy ra danh sách bài hát, SẮP XẾP theo PositionOrder
                    Songs = p.PlaylistSongs
                        .Where(ps => ps.Song.IsDeleted == false) // Không lấy bài đã bị xóa
                        .OrderBy(ps => ps.PositionOrder)
                        .Select(ps => new SongResponse
                        {
                            SongId = ps.Song.SongId,
                            Title = ps.Song.Title,
                            DurationSeconds = ps.Song.DurationSeconds,
                            ReleaseDate = ps.Song.ReleaseDate,
                            TotalPlays = ps.Song.TotalPlays,
                            // Kéo theo file MP3
                            AudioFiles = ps.Song.AudioFiles.Select(a => new SongAudioFileResponse
                            {
                                FileId = a.FileId,
                                Quality = a.Quality,
                                FileUrl = a.FileUrl,
                                SizeBytes = a.SizeBytes
                            }).ToList(),
                            // Kéo theo Nghệ sĩ
                            Artists = ps.Song.ArtistSongs.Select(ast => new SongArtistResponse
                            {
                                ArtistId = ast.ArtistId,
                                Name = ast.Artist.Name,
                                Role = ast.Role
                            }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (playlist == null) throw new Exception("Playlist không tồn tại hoặc đã bị xóa.");

            // BẢO MẬT: Nếu Playlist là Private, chỉ có chủ sở hữu mới được xem
            if (playlist.IsPublic == false && playlist.UserId != currentUserId)
            {
                throw new Exception("Đây là danh sách phát riêng tư. Bạn không có quyền truy cập.");
            }

            return playlist;
        }

        // --- HÀM 2: XÓA BÀI HÁT KHỎI PLAYLIST ---
        public async Task<bool> RemoveSongFromPlaylistAsync(string userId, string playlistId, string songId)
        {
            // 1. Kiểm tra Playlist có tồn tại và đúng là của User này không
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null || playlist.IsDeleted == true)
                throw new Exception("Playlist không tồn tại.");
            if (playlist.UserId != userId)
                throw new Exception("Bạn không có quyền chỉnh sửa Playlist này.");

            // 2. Tìm bài hát trong Playlist
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
                throw new Exception("Bài hát không tồn tại trong Playlist này.");

            // 3. Xóa khỏi DB
            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return true;
        }
    }



}