using project_music.DTOs.Downloads;

namespace project_music.Services.Downloads
{
    public interface IDownloadService
    {
        // Hàm xử lý logic cấp phép tải nhạc
        Task<DownloadResponse> ProcessDownloadAsync(string userId, DownloadRequest request);
    }
}