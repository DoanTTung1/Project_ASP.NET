using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Songs
{
    public class SongService : ISongService
    {
        private readonly MusicDbContext _context;

        public SongService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<SongResponse>> GetAllAsync()
        {
            return await _context.Songs
                .Where(s => s.IsDeleted == false)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    TotalPlays = s.TotalPlays,
                    AlbumId = s.AlbumId,
                    // GỘP DỮ LIỆU TỪ BẢNG AUDIO_FILES VÀO ĐÂY
                    AudioFiles = s.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<SongResponse?> GetByIdAsync(string id)
        {
            // Thay vì dùng FindAsync, ta dùng truy vấn LINQ để lấy kèm AudioFiles luôn
            var song = await _context.Songs
                .Where(s => s.SongId == id && s.IsDeleted == false)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    TotalPlays = s.TotalPlays,
                    AlbumId = s.AlbumId,
                    // GỘP DỮ LIỆU TỪ BẢNG AUDIO_FILES VÀO ĐÂY
                    AudioFiles = s.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes
                    }).ToList()
                }).FirstOrDefaultAsync();

            return song;
        }

        public async Task<SongResponse> CreateAsync(CreateSongRequest request)
        {
            var newSong = new Song
            {
                SongId = Guid.NewGuid().ToString(),
                Title = request.Title,
                DurationSeconds = request.DurationSeconds,
                ReleaseDate = request.ReleaseDate,
                AlbumId = request.AlbumId,
                TotalPlays = 0,
                IsDeleted = false
            };

            _context.Songs.Add(newSong);
            await _context.SaveChangesAsync();

            return new SongResponse
            {
                SongId = newSong.SongId,
                Title = newSong.Title,
                DurationSeconds = newSong.DurationSeconds,
                ReleaseDate = newSong.ReleaseDate,
                TotalPlays = newSong.TotalPlays,
                AlbumId = newSong.AlbumId
            };
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null || song.IsDeleted == true) return false;

            // XÓA MỀM: Không dùng _context.Songs.Remove(song);
            // Thay vào đó, ta cập nhật trạng thái IsDeleted
            song.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}