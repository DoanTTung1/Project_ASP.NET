using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Search;
using project_music.Models;

namespace project_music.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly MusicDbContext _context;

        public SearchService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResponse> SearchAllAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new SearchResponse(); // Nếu không nhập gì thì trả về rỗng luôn
            }

            // Chuyển từ khóa về chữ thường để tìm kiếm không phân biệt hoa thường
            var lowerKeyword = keyword.ToLower();

            // 1. Tìm Bài hát (Giới hạn 10 bài)
            var songs = await _context.Songs
                .Where(s => s.Title.ToLower().Contains(lowerKeyword) && s.IsDeleted == false)
                .Take(10)
                .Select(s => new SearchSongResponse
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    // Lấy tên ca sĩ có vai trò MAIN đầu tiên
                    ArtistName = s.ArtistSongs.Where(ast => ast.Role == "MAIN").Select(ast => ast.Artist.Name).FirstOrDefault(),
                    CoverUrl = s.Album != null ? s.Album.CoverUrl : null
                })
                .ToListAsync();

            // 2. Tìm Ca sĩ (Giới hạn 5 người)
            var artists = await _context.Artists
                .Where(a => a.Name.ToLower().Contains(lowerKeyword) && a.IsDeleted == false)
                .Take(5)
                .Select(a => new SearchArtistResponse
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    AvatarUrl = a.AvatarUrl
                })
                .ToListAsync();

            // 3. Tìm Playlist (Chỉ tìm các Playlist CÔNG KHAI, giới hạn 5 cái)
            var playlists = await _context.Playlists
                .Where(p => p.Name.ToLower().Contains(lowerKeyword) && p.IsPublic == true && p.IsDeleted == false)
                .Take(5)
                .Select(p => new SearchPlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username
                })
                .ToListAsync();

            // Gộp tất cả lại và trả về
            return new SearchResponse
            {
                Songs = songs,
                Artists = artists,
                Playlists = playlists
            };
        }
    }
}