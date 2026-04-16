using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Social;
using project_music.Models;

namespace project_music.Services.Social
{
    public class SocialService : ISocialService
    {
        private readonly MusicDbContext _context;

        public SocialService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollowArtistAsync(string userId, string artistId)
        {
            // 1. Kiểm tra Ca sĩ có tồn tại không
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null || artist.IsDeleted == true)
                throw new Exception("Ca sĩ không tồn tại hoặc đã bị xóa.");

            // 2. Tìm xem đã Follow chưa
            var followRecord = await _context.UserFollowArtists
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);

            if (followRecord != null)
            {
                // NẾU ĐÃ FOLLOW RỒI -> Xóa đi (Unfollow)
                _context.UserFollowArtists.Remove(followRecord);
                await _context.SaveChangesAsync();
                return false; // Trả về false báo hiệu là đã Unfollow
            }
            else
            {
                // NẾU CHƯA FOLLOW -> Thêm vào danh sách
                var newFollow = new UserFollowArtist
                {
                    UserId = userId,
                    ArtistId = artistId,
                    FollowedAt = DateTime.UtcNow
                };
                _context.UserFollowArtists.Add(newFollow);
                await _context.SaveChangesAsync();
                return true; // Trả về true báo hiệu là đã Follow
            }
        }

        public async Task<List<FollowedArtistResponse>> GetFollowedArtistsAsync(string userId)
        {
            // Lấy danh sách ca sĩ mà người dùng này đang theo dõi
            var followedArtists = await _context.UserFollowArtists
                .Where(f => f.UserId == userId && f.Artist.IsDeleted == false)
                .OrderByDescending(f => f.FollowedAt) // Ai mới follow thì xếp lên đầu
                .Select(f => new FollowedArtistResponse
                {
                    ArtistId = f.ArtistId,
                    Name = f.Artist.Name,
                    AvatarUrl = f.Artist.AvatarUrl,
                    FollowedAt = f.FollowedAt
                })
                .ToListAsync();

            return followedArtists;
        }
    }
}