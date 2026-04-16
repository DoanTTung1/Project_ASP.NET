using project_music.DTOs.History;

namespace project_music.Services.History
{
    public interface IHistoryService
    {
        // Ghi lại 1 lượt nghe nhạc
        Task<bool> RecordPlayAsync(string userId, RecordPlayRequest request);

        // Lấy danh sách 20 bài hát nghe gần đây nhất
        Task<List<HistoryResponse>> GetRecentlyPlayedAsync(string userId);
    }
}