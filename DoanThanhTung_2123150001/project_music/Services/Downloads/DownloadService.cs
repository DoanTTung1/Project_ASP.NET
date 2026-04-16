using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Downloads;
using project_music.Models;

namespace project_music.Services.Downloads
{
    public class DownloadService : IDownloadService
    {
        private readonly MusicDbContext _context;

        public DownloadService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<DownloadResponse> ProcessDownloadAsync(string userId, DownloadRequest request)
        {
            // 1. Lấy thông tin User và KIỂM TRA QUYỀN VIP
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            if (user.IsPremium == false || user.PremiumExpiryDate == null || user.PremiumExpiryDate < DateTime.UtcNow)
            {
                throw new Exception("Tính năng tải nhạc Offline chỉ dành cho tài khoản Premium đang có hiệu lực. Vui lòng nâng cấp gói cước.");
            }

            // 2. Kiểm tra Bài hát có tồn tại không
            var song = await _context.Songs.FindAsync(request.SongId);
            if (song == null || song.IsDeleted == true)
                throw new Exception("Bài hát không tồn tại.");

            // 3. Tìm File âm thanh chất lượng cao nhất của bài này (LOSSLESS -> 320KBPS -> 128KBPS)
            var bestAudioFile = await _context.AudioFiles
                .Where(a => a.SongId == request.SongId)
                .OrderBy(a => a.Quality == "LOSSLESS" ? 1 : a.Quality == "320KBPS" ? 2 : 3)
                .FirstOrDefaultAsync();

            if (bestAudioFile == null)
                throw new Exception("Bài hát này hiện chưa có file âm thanh để tải.");

            // 4. Lưu vết tải xuống vào Database (Bảo mật DRM)
            var downloadRecord = new UserDownload
            {
                DownloadId = Guid.NewGuid().ToString(),
                UserId = userId,
                SongId = request.SongId,
                DeviceId = request.DeviceId,
                DownloadedAt = DateTime.UtcNow,

                // QUAN TRỌNG: File này chỉ sống được đến ngày VIP hết hạn!
                ExpiresAt = user.PremiumExpiryDate
            };

            _context.UserDownloads.Add(downloadRecord);
            await _context.SaveChangesAsync();

            // 5. Trả kết quả về cho App Mobile / Web tải
            return new DownloadResponse
            {
                DownloadId = downloadRecord.DownloadId,
                SongId = song.SongId,
                Title = song.Title,
                FileUrl = bestAudioFile.FileUrl,
                Quality = bestAudioFile.Quality,
                ExpiresAt = downloadRecord.ExpiresAt
            };
        }
    }
}