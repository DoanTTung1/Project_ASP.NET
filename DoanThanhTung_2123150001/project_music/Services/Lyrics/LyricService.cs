using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Lyrics;
using project_music.Models;

namespace project_music.Services.Lyrics
{
    public class LyricService : ILyricService
    {
        private readonly MusicDbContext _context;

        public LyricService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<LyricResponse>> GetLyricsBySongIdAsync(string songId)
        {
            // Kiểm tra bài hát có tồn tại không
            var songExists = await _context.Songs.AnyAsync(s => s.SongId == songId && s.IsDeleted == false);
            if (!songExists)
                throw new Exception("Bài hát không tồn tại hoặc đã bị xóa.");

            // Lấy tất cả các phiên bản lời bài hát (Việt, Anh...) của bài này
            var lyrics = await _context.SongLyrics
                .Where(l => l.SongId == songId)
                .Select(l => new LyricResponse
                {
                    LyricId = l.LyricId,
                    SongId = l.SongId,
                    Language = l.Language ?? "vi",
                    SyncType = l.SyncType ?? "UNSYNCED",
                    Content = l.Content
                })
                .ToListAsync();

            return lyrics;
        }
    }
}