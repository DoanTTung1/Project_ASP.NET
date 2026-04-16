using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Admin;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly MusicDbContext _context;

        public AdminService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
        {
            // 1. Đếm tổng User
            var totalUsers = await _context.Users.CountAsync(u => u.IsDeleted == false);

            // 2. Đếm User đang là VIP
            var premiumUsers = await _context.Users.CountAsync(u => u.IsPremium == true && u.IsDeleted == false);

            // 3. Tính tổng doanh thu (Chỉ cộng những giao dịch SUCCESS)
            var totalRevenue = await _context.Transactions
                .Where(t => t.Status == "SUCCESS")
                .SumAsync(t => t.Amount);

            // 4. Lấy Top 5 bài hát nhiều View nhất
            var topSongs = await _context.Songs
                .Where(s => s.IsDeleted == false)
                .OrderByDescending(s => s.TotalPlays)
                .Take(5)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    TotalPlays = s.TotalPlays,
                    ReleaseDate = s.ReleaseDate
                })
                .ToListAsync();

            return new DashboardStatsResponse
            {
                TotalUsers = totalUsers,
                TotalPremiumUsers = premiumUsers,
                TotalRevenue = totalRevenue,
                TopTrendingSongs = topSongs
            };
        }
    }
}