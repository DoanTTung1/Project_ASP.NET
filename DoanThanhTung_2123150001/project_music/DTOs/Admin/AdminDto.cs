using project_music.DTOs.Songs;

namespace project_music.DTOs.Admin
{
    public class DashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalPremiumUsers { get; set; }
        public decimal TotalRevenue { get; set; }

        // Kéo theo danh sách 5 bài hát hot nhất
        public List<SongResponse> TopTrendingSongs { get; set; } = new List<SongResponse>();
    }
}