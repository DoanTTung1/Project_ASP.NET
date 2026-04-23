using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Playlists;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlaylistService(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 👉 HÀM BỔ TRỢ: Lưu file vào thư mục uploads
        private async Task<string?> SaveFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "playlists");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/playlists/{fileName}";
        }

        public async Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request)
        {
            string? coverUrl = await SaveFileAsync(request.CoverFile);

            var newPlaylist = new Playlist
            {
                PlaylistId = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                IsPublic = request.IsPublic,
                CoverUrl = coverUrl,
                IsSystemPlaylist = request.IsSystemPlaylist, // Bổ sung
                IsVipOnly = request.IsVipOnly,               // Bổ sung
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
                IsSystemPlaylist = newPlaylist.IsSystemPlaylist, // Bổ sung
                IsVipOnly = newPlaylist.IsVipOnly,               // Bổ sung
                CreatedAt = newPlaylist.CreatedAt,
                CreatorName = user?.Username ?? "Unknown",
                TotalSongs = 0
            };
        }

        public async Task<List<PlaylistResponse>> GetAllPlaylistsAsync()
        {
            return await _context.Playlists
                .Where(p => p.IsDeleted == false)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    IsPublic = p.IsPublic ?? false,
                    IsSystemPlaylist = p.IsSystemPlaylist, // Bổ sung
                    IsVipOnly = p.IsVipOnly,               // Bổ sung
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count()
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId)
        {
            return await _context.Playlists
                .Where(p => p.UserId == userId && p.IsDeleted == false)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    IsPublic = p.IsPublic ?? false,
                    IsSystemPlaylist = p.IsSystemPlaylist, // Bổ sung
                    IsVipOnly = p.IsVipOnly,               // Bổ sung
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count()
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddSongToPlaylistAsync(string userId, string playlistId, string songId)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null || playlist.IsDeleted == true) throw new Exception("Playlist không tồn tại.");

            // Nếu là Playlist hệ thống thì Admin (người có userId khác người tạo ban đầu) 
            // Vẫn được phép thêm bài hát, do đó ta cần nới lỏng điều kiện này
            if (playlist.UserId != userId && playlist.IsSystemPlaylist == false)
                throw new Exception("Không có quyền thêm nhạc vào playlist của người khác.");

            var exists = await _context.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (exists) throw new Exception("Bài hát đã có sẵn trong Playlist này.");

            // 👉 FIX LỖI 500 TẠI ĐÂY: Xử lý an toàn khi Playlist chưa có bài hát nào
            int currentMaxPosition = 0;
            var hasAnySong = await _context.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId);
            if (hasAnySong)
            {
                currentMaxPosition = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == playlistId)
                    .MaxAsync(ps => ps.PositionOrder);
            }

            _context.PlaylistSongs.Add(new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                PositionOrder = currentMaxPosition + 1
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePlaylistAsync(string userId, string playlistId, CreatePlaylistRequest request)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId && p.IsDeleted == false);
            if (playlist == null) throw new Exception("Không tìm thấy Playlist.");

            playlist.Name = request.Name;
            playlist.Description = request.Description;
            playlist.IsPublic = request.IsPublic;
            playlist.IsSystemPlaylist = request.IsSystemPlaylist; // Bổ sung
            playlist.IsVipOnly = request.IsVipOnly;               // Bổ sung

            if (request.CoverFile != null)
            {
                var newUrl = await SaveFileAsync(request.CoverFile);
                if (newUrl != null) playlist.CoverUrl = newUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlaylistAsync(string userId, string playlistId)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId && p.IsDeleted == false);
            if (playlist == null) return false;
            playlist.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

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
                    IsSystemPlaylist = p.IsSystemPlaylist, // Bổ sung
                    IsVipOnly = p.IsVipOnly,               // Bổ sung
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count(),
                    Songs = p.PlaylistSongs.Where(ps => ps.Song.IsDeleted == false).OrderBy(ps => ps.PositionOrder)
                        .Select(ps => new SongResponse
                        {
                            SongId = ps.Song.SongId,
                            Title = ps.Song.Title,
                            DurationSeconds = ps.Song.DurationSeconds,
                            CoverUrl = ps.Song.CoverUrl,
                            IsVip = ps.Song.IsVip,
                            // 👉 Kéo FileUrl ra đây để Frontend dùng chạy nhạc
                            FileUrl = ps.Song.AudioFiles.Select(a => a.FileUrl).FirstOrDefault(),
                            Artists = ps.Song.ArtistSongs.Select(ast => new SongArtistResponse { ArtistId = ast.ArtistId, Name = ast.Artist.Name }).ToList()
                        }).ToList()
                }).FirstOrDefaultAsync();

            if (playlist == null) throw new Exception("Không tồn tại.");
            if (playlist.IsPublic == false && playlist.UserId != currentUserId) throw new Exception("Riêng tư.");

            // 👉 LOGIC PHÂN QUYỀN VIP
            playlist.HasFullAccess = true;
            if (playlist.IsVipOnly)
            {
                playlist.HasFullAccess = false; // Mặc định khóa nếu là Playlist VIP

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var user = await _context.Users.FindAsync(currentUserId);
                    // Dựa vào DB của Boss, thuộc tính là IsPremium
                    if (user != null && (user.IsPremium == true || user.Role == "Admin" || playlist.UserId == currentUserId))
                    {
                        playlist.HasFullAccess = true; // Mở khóa nếu là Premium, Admin hoặc Chủ Playlist
                    }
                }
            }

            return playlist;
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(string userId, string playlistId, string songId)
        {
            var ps = await _context.PlaylistSongs.FirstOrDefaultAsync(x => x.PlaylistId == playlistId && x.SongId == songId && x.Playlist.UserId == userId);
            if (ps == null) throw new Exception("Lỗi xóa bài hát.");
            _context.PlaylistSongs.Remove(ps);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}