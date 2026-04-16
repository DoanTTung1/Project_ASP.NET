using project_music.DTOs.Home;

namespace project_music.Services.Home
{
    public interface IHomeService
    {
        Task<HomeResponse> GetHomeDataAsync();
    }
}