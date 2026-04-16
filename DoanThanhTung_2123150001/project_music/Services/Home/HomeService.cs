using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Home;
using project_music.DTOs.Playlists;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly MusicDbContext _context;

        public HomeService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<HomeResponse> GetHomeDataAsync()
        {
            // 1. Lấy 5 bài hát MỚI NHẤT (Sắp xếp theo Ngày phát hành)
            var newReleases = await _context.Songs
                .Where(s => s.IsDeleted == false)
                .OrderByDescending(s => s.ReleaseDate)
                .Take(5)
                .Select(s => new SongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    DurationSeconds = s.DurationSeconds,
                    ReleaseDate = s.ReleaseDate,
                    CoverUrl = s.Album != null ? s.Album.CoverUrl : null,

                    // Đã lấy được link Audio cực chuẩn!
                    FileUrl = _context.AudioFiles.Where(a => a.SongId == s.SongId).Select(a => a.FileUrl).FirstOrDefault(),

                    // ĐÃ SỬA LẠI: Trả lại tên cho Ca sĩ!
                    Artists = s.ArtistSongs.Select(ast => new SongArtistResponse
                    {
                        ArtistId = ast.ArtistId,
                        Name = ast.Artist.Name,
                        Role = ast.Role
                    }).ToList()
                })
                .ToListAsync();

            // 2. Lấy 5 Playlist CÔNG KHAI nổi bật nhất
            var featuredPlaylists = await _context.Playlists
                .Where(p => p.IsPublic == true && p.IsDeleted == false && p.PlaylistSongs.Any())
                .OrderByDescending(p => p.PlaylistSongs.Count)
                .Take(5)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count()
                })
                .ToListAsync();

            // 3. Lấy danh sách Thể loại nhạc (Tất cả)
            var genres = await _context.Genres
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    ImageUrl = g.ImageUrl
                })
                .ToListAsync();

            // Gộp tất cả vào cái "Rổ Bự" trả về cho Front-end
            return new HomeResponse
            {
                NewReleases = newReleases,
                FeaturedPlaylists = featuredPlaylists,
                Genres = genres
            };
        }
    }
}