using Microsoft.EntityFrameworkCore; // 1. THÊM DÒNG NÀY VÀO ĐỂ CHẠY ĐƯỢC TO LIST ASYNC
using project_music.DTOs.Playlists;
using project_music.Models;

namespace project_music.Services.Playlists
{
    
    public class PlaylistService : IPlaylistService
    {
        private readonly MusicDbContext _context;
        public PlaylistService(MusicDbContext context)
        {
            _context = context;
        }

    
        public async Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request)
        {
            var newPlaylist = new Playlist
            {
                PlaylistId = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Playlists.Add(newPlaylist);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new PlaylistResponse
            {
                PlaylistId = newPlaylist.PlaylistId,
                UserId = newPlaylist.UserId,
                CoverUrl = newPlaylist.CoverUrl,
                Name = newPlaylist.Name,
                Description = newPlaylist.Description,
                IsPublic = newPlaylist.IsPublic ?? false,
                CreatedAt = newPlaylist.CreatedAt,
                CreatorName = user?.Username ?? "Unknown",
                TotalSongs = 0
            };
        }

        public async Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId)
        {
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId && p.IsDeleted == false)
                .Select(p => new PlaylistResponse
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Description = p.Description,
                    IsPublic = p.IsPublic ?? false,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    CoverUrl = p.CoverUrl,
                    CreatorName = p.User.Username,
                    TotalSongs = p.PlaylistSongs.Count()
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(); 

            return playlists;
        }
    }
}