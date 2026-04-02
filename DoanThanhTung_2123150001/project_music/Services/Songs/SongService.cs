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
                    }).ToList(),
                    Artists = s.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<SongResponse?> GetByIdAsync(string id)
        {
            
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
                    }).ToList(),
                    Artists = s.ArtistSongs.Select(x => new SongArtistResponse
                    {
                        ArtistId = x.ArtistId,
                        Name = x.Artist.Name,
                        Role = x.Role
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
            if(request.Artists !=null && request.Artists.Any())
            {
                foreach(var i in request.Artists)
                {
                    var artistExists = await _context.Artists.AnyAsync(a => a.ArtistId == i.ArtistId);
                    if (!artistExists) throw new Exception($"Không tìm thấy ca sĩ có mã: {i.ArtistId}");
                    var artistSong = new ArtistSong
                    {
                        ArtistId = i.ArtistId,
                        SongId = newSong.SongId,
                        Role = i.Role
                    };
                    _context.ArtistSongs.Add(artistSong);
                }
            }
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newSong.SongId) ?? throw new Exception("Lỗi khi tải lại dữ liệu bài hát");
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

        public async Task<bool> ToggleFavoriteAsync(string userId, string songId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if(song == null || song.IsDeleted == true) throw new Exception("Bài hát không tồn tại");
            var favorite = await _context.UserFavoriteSongs.FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);
            if(favorite !=null)
            {
                _context.UserFavoriteSongs.Remove(favorite);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                var newFavorite = new UserFavoriteSong
                {
                    UserId = userId,
                    SongId = songId
                };
                _context.UserFavoriteSongs.Add(newFavorite);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<SongResponse>> GetMyFavoriteSongsAsync(string userId)
        {
            var favoriteSongs = await _context.UserFavoriteSongs
                .Where(f => f.UserId == userId && f.Song.IsDeleted == false)
                .Select(f => new SongResponse
                {
                    SongId = f.Song.SongId,
                    Title = f.Song.Title,
                    DurationSeconds = f.Song.DurationSeconds,
                    ReleaseDate = f.Song.ReleaseDate,
                    TotalPlays = f.Song.TotalPlays,
                    AlbumId = f.Song.AlbumId,
                    AudioFiles = f.Song.AudioFiles.Select(a => new SongAudioFileResponse
                    {
                        FileId = a.FileId,
                        Quality = a.Quality,
                        FileUrl = a.FileUrl,
                        SizeBytes = a.SizeBytes

                    }).ToList(),

                    Artists = f.Song.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                }).ToListAsync();
            return favoriteSongs;
        }
    }
}
