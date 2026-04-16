using project_music.DTOs.Lyrics;

namespace project_music.Services.Lyrics
{
    public interface ILyricService
    {
        // Lấy danh sách lời bài hát của 1 bài (vì có thể có nhiều ngôn ngữ: vi, en, kr...)
        Task<List<LyricResponse>> GetLyricsBySongIdAsync(string songId);
    }
}