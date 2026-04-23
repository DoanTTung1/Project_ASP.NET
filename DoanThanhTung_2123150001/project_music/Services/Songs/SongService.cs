using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Songs;
using project_music.Models;
using System.Security.Claims;

namespace project_music.Services.Songs
{
    public class SongService : ISongService
    {
        private readonly MusicDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; // 👉 Thêm cái này để lấy thông tin Token của User

        public SongService(MusicDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 👉 HÀM BỔ TRỢ: Kiểm tra xem người đang dùng App có phải Premium hoặc Admin không
        private async Task<bool> CheckUserHasPremiumAccess()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false; // Khách vãng lai -> False

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            return user.IsPremium == true || user.Role == "Admin"; // VIP hoặc Admin -> True
        }

        public async Task<List<SongResponse>> GetAllAsync()
        {
            bool isPremiumUser = await CheckUserHasPremiumAccess();

            return await _context.Songs
                .Where(s => s.IsDeleted == false)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    TotalPlays = s.TotalPlays,
                    AlbumId = s.AlbumId,
                    CoverUrl = s.CoverUrl,
                    IsVip = s.IsVip, // 👉 Đẩy nhãn VIP ra ngoài
                    HasFullAccess = s.IsVip ? isPremiumUser : true, // 👉 Nếu là VIP thì xét quyền, nếu bài Free thì luôn True
                    Lyrics = s.SongLyrics.Select(l => l.Content).FirstOrDefault(),
                    Genres = s.Genres.Select(g => g.Name).ToList(),
                    AudioFiles = s.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes
                    }).ToList(),
                    Artists = s.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<SongResponse?> GetByIdAsync(string id)
        {
            bool isPremiumUser = await CheckUserHasPremiumAccess();

            return await _context.Songs
                .Where(s => s.SongId == id && s.IsDeleted == false)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    TotalPlays = s.TotalPlays,
                    AlbumId = s.AlbumId,
                    CoverUrl = s.CoverUrl,
                    IsVip = s.IsVip, // 👉 Đẩy nhãn VIP ra ngoài
                    HasFullAccess = s.IsVip ? isPremiumUser : true, // 👉 Cờ cắt nhạc
                    Lyrics = s.SongLyrics.Select(l => l.Content).FirstOrDefault(),
                    Genres = s.Genres.Select(g => g.Name).ToList(),
                    AudioFiles = s.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes
                    }).ToList(),
                    Artists = s.ArtistSongs.Select(x => new SongArtistResponse
                    {
                        ArtistId = x.ArtistId,
                        Name = x.Artist.Name,
                        Role = x.Role
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<SongResponse> CreateAsync(CreateSongRequest request)
        {
            var newSong = new Song
            {
                SongId = Guid.NewGuid().ToString(),
                Title = request.Title,
                DurationSeconds = request.DurationSeconds,
                ReleaseDate = request.ReleaseDate,
                AlbumId = request.AlbumId,
                IsVip = request.IsVip, // 👉 Gán mác VIP khi tạo mới
                TotalPlays = 0,
                IsDeleted = false
            };

            if (!string.IsNullOrWhiteSpace(request.Lyrics))
            {
                newSong.SongLyrics.Add(new SongLyric
                {
                    LyricId = Guid.NewGuid().ToString(),
                    Content = request.Lyrics,
                    Language = "vi"
                });
            }

            if (request.GenreIds != null && request.GenreIds.Any())
            {
                var genres = await _context.Genres.Where(g => request.GenreIds.Contains(g.GenreId)).ToListAsync();
                foreach (var genre in genres)
                {
                    newSong.Genres.Add(genre);
                }
            }

            _context.Songs.Add(newSong);

            if (request.Artists != null && request.Artists.Any())
            {
                foreach (var i in request.Artists)
                {
                    _context.ArtistSongs.Add(new ArtistSong
                    {
                        ArtistId = i.ArtistId,
                        SongId = newSong.SongId,
                        Role = i.Role
                    });
                }
            }
            await _context.SaveChangesAsync();
            return await GetByIdAsync(newSong.SongId) ?? throw new Exception("Lỗi tải lại dữ liệu");
        }

        public async Task<SongResponse> UpdateAsync(string id, UpdateSongRequest request)
        {
            var song = await _context.Songs
                .Include(s => s.ArtistSongs)
                .Include(s => s.SongLyrics)
                .Include(s => s.Genres)
                .FirstOrDefaultAsync(s => s.SongId == id && s.IsDeleted == false);

            if (song == null) throw new Exception("Không tìm thấy bài hát.");

            song.Title = request.Title;
            song.DurationSeconds = request.DurationSeconds;
            song.IsVip = request.IsVip; // 👉 Cập nhật nhãn VIP
            song.ReleaseDate = request.ReleaseDate.HasValue ? DateOnly.FromDateTime(request.ReleaseDate.Value) : null;

            var existingLyric = song.SongLyrics.FirstOrDefault();
            if (existingLyric != null)
            {
                existingLyric.Content = request.Lyrics ?? "";
            }
            else if (!string.IsNullOrWhiteSpace(request.Lyrics))
            {
                song.SongLyrics.Add(new SongLyric { LyricId = Guid.NewGuid().ToString(), Content = request.Lyrics, Language = "vi" });
            }

            _context.ArtistSongs.RemoveRange(song.ArtistSongs);
            foreach (var artistId in request.ArtistIds)
            {
                _context.ArtistSongs.Add(new ArtistSong { SongId = id, ArtistId = artistId, Role = "MAIN" });
            }

            song.Genres.Clear();
            if (request.GenreIds != null && request.GenreIds.Any())
            {
                var genres = await _context.Genres.Where(g => request.GenreIds.Contains(g.GenreId)).ToListAsync();
                foreach (var genre in genres)
                {
                    song.Genres.Add(genre);
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? throw new Exception("Lỗi tải lại dữ liệu");
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null || song.IsDeleted == true) return false;
            song.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, string songId)
        {
            var favorite = await _context.UserFavoriteSongs.FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);
            if (favorite != null)
            {
                _context.UserFavoriteSongs.Remove(favorite);
                await _context.SaveChangesAsync();
                return false;
            }
            _context.UserFavoriteSongs.Add(new UserFavoriteSong { UserId = userId, SongId = songId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SongResponse>> GetMyFavoriteSongsAsync(string userId)
        {
            bool isPremiumUser = await CheckUserHasPremiumAccess();

            return await _context.UserFavoriteSongs
                .Where(f => f.UserId == userId && f.Song.IsDeleted == false)
                .Select(f => new SongResponse
                {
                    SongId = f.Song.SongId,
                    Title = f.Song.Title,
                    DurationSeconds = f.Song.DurationSeconds,
                    CoverUrl = f.Song.CoverUrl,
                    IsVip = f.Song.IsVip,
                    HasFullAccess = f.Song.IsVip ? isPremiumUser : true,

                    // 👉 MẤU CHỐT LÀ ĐÂY: Thêm 2 dòng này để lấy link nhạc
                    FileUrl = f.Song.AudioFiles.Select(a => a.FileUrl).FirstOrDefault(),
                    AudioFiles = f.Song.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes
                    }).ToList(),

                    Lyrics = f.Song.SongLyrics.Select(l => l.Content).FirstOrDefault(),
                    Genres = f.Song.Genres.Select(g => g.Name).ToList(),
                    Artists = f.Song.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name
                    }).ToList()
                }).ToListAsync();
        }
    }
}