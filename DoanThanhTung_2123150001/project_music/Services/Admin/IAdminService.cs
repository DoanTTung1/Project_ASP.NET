using project_music.DTOs.Admin;

namespace project_music.Services.Admin
{
    public interface IAdminService
    {
        Task<DashboardStatsResponse> GetDashboardStatsAsync();
    }
}