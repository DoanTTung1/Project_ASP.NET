using Microsoft.EntityFrameworkCore;
using project_music.DTOs.History;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.History
{
    public class HistoryService : IHistoryService
    {
        private readonly MusicDbContext _context;

        public HistoryService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RecordPlayAsync(string userId, RecordPlayRequest request)
        {
            // 1. Kiểm tra bài hát có thật không
            var song = await _context.Songs.FindAsync(request.SongId);
            if (song == null || song.IsDeleted == true)
                throw new Exception("Bài hát không tồn tại.");

            // 2. Lưu vào bảng lịch sử
            var history = new PlayHistory
            {
                HistoryId = Guid.NewGuid().ToString(),
                UserId = userId,
                SongId = request.SongId,
                ListenDurationSeconds = request.ListenDurationSeconds,
                PlayedAt = DateTime.UtcNow
            };

            _context.PlayHistories.Add(history);

            // 3. Tăng biến đếm tổng lượt nghe của bài hát (Total Plays)
            // Cứ nghe trên 30 giây thì được tính là 1 lượt view (Chuẩn Spotify)
            if (request.ListenDurationSeconds >= 30)
            {
                song.TotalPlays = (song.TotalPlays ?? 0) + 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<HistoryResponse>> GetRecentlyPlayedAsync(string userId)
        {
            var history = await _context.PlayHistories
                .Where(h => h.UserId == userId && h.Song.IsDeleted == false)
                .OrderByDescending(h => h.PlayedAt) // Lấy những bài nghe gần nhất xếp lên đầu
                .Take(20) // Chỉ lấy 20 bài gần nhất để khỏi lag app
                .Select(h => new HistoryResponse
                {
                    HistoryId = h.HistoryId,
                    SongId = h.SongId,
                    Title = h.Song.Title,
                    PlayedAt = h.PlayedAt,
                    ListenDurationSeconds = h.ListenDurationSeconds,
                    Artists = h.Song.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                })
                .ToListAsync();

            return history;
        }
    }
}