using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Home;
using project_music.DTOs.Playlists;
using project_music.DTOs.Songs;
using project_music.Models;
using System.Security.Claims;

namespace project_music.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly MusicDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; // 👉 Cần cái này để check VIP

        public HomeService(MusicDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 👉 HÀM CHECK QUYỀN VIP
        private async Task<bool> CheckUserHasPremiumAccess()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false; // Khách vãng lai

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            return user.IsPremium == true || user.Role == "Admin"; // VIP hoặc Admin thì được
        }

        public async Task<HomeResponse> GetHomeDataAsync()
        {
            // Kiểm tra xem thằng đang truy cập trang chủ có phải là VIP/Admin không
            bool isPremiumUser = await CheckUserHasPremiumAccess();

            // 1. Lấy 5 bài hát MỚI NHẤT (Sắp xếp theo Ngày phát hành)
            var newReleases = await _context.Songs
                .Where(s => s.IsDeleted == false)
                .OrderByDescending(s => s.ReleaseDate)
                .Take(5)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    CoverUrl = s.CoverUrl,

                    // 👉 TRỌNG TÂM LÀ ĐÂY: Gắn nhãn VIP và cờ Cắt nhạc
                    IsVip = s.IsVip,
                    HasFullAccess = s.IsVip ? isPremiumUser : true,

                    // Đã lấy được link Audio cực chuẩn!
                    FileUrl = _context.AudioFiles.Where(a => a.SongId == s.SongId).Select(a => a.FileUrl).FirstOrDefault(),

                    // Trả lại tên cho Ca sĩ!
                    Artists = s.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                })
                .ToListAsync();

            // 2. Lấy 5 Playlist CÔNG KHAI nổi bật nhất
            var featuredPlaylists = await _context.Playlists
                .Where(p => p.IsPublic == true && p.IsDeleted == false && p.PlaylistSongs.Any())
                .OrderByDescending(p => p.PlaylistSongs.Count)
                .Take(5)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count(),

                    // 👉 Sẵn tiện tôi gắn luôn cờ IsVipOnly cho Playlist ngoài trang chủ 
                    // để lỡ Boss có cài VIP cho nguyên cái Playlist thì nó hiện chữ "👑 VIP"
                    IsVipOnly = p.IsVipOnly
                })
                .ToListAsync();

            // 3. Lấy danh sách Thể loại nhạc (Tất cả)
            var genres = await _context.Genres
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    ImageUrl = g.ImageUrl
                })
                .ToListAsync();

            // Gộp tất cả vào cái "Rổ Bự" trả về cho Front-end
            return new HomeResponse
            {
                NewReleases = newReleases,
                FeaturedPlaylists = featuredPlaylists,
                Genres = genres
            };
        }
    }
}