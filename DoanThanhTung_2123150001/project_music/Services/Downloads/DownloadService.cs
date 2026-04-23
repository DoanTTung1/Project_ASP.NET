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
            // 1. Kiểm tra User
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            // 2. Kiểm tra Bài hát
            var song = await _context.Songs.FindAsync(request.SongId);
            if (song == null || song.IsDeleted == true)
                throw new Exception("Bài hát không tồn tại.");

            // 3. CHẶN NHẠC VIP
            if (song.IsVip)
            {
                throw new Exception("👑 Đây là nội dung Premium Độc Quyền. Bạn chỉ có thể nghe trực tuyến, không được phép tải xuống thiết bị!");
            }

            // 4. Lấy file âm thanh
            var audioFiles = await _context.AudioFiles
                .Where(a => a.SongId == request.SongId)
                .ToListAsync();

            var bestAudioFile = audioFiles
                .OrderBy(a => a.Quality == "LOSSLESS" ? 1 : (a.Quality == "320KBPS" ? 2 : 3))
                .FirstOrDefault();

            if (bestAudioFile == null)
                throw new Exception("Bài hát này hiện chưa có file âm thanh trên hệ thống để tải.");

            // 5. Lưu lịch sử tải xuống (BỌC BẢO VỆ CHỐNG SẬP DATABASE)
            string downloadId = Guid.NewGuid().ToString();
            try
            {
                var safeDeviceId = string.IsNullOrEmpty(request.DeviceId) ? "WEB_BROWSER" : request.DeviceId;
                if (safeDeviceId.Length > 50) safeDeviceId = safeDeviceId.Substring(0, 50);

                var downloadRecord = new UserDownload
                {
                    DownloadId = downloadId,
                    UserId = userId,
                    SongId = request.SongId,
                    DeviceId = safeDeviceId,
                    DownloadedAt = DateTime.UtcNow,
                    // Nếu cột ExpiresAt trong DB của Boss bắt buộc phải có giá trị, 
                    // ta set mặc định 100 năm sau thay vì để null
                    ExpiresAt = DateTime.UtcNow.AddYears(100)
                };

                _context.UserDownloads.Add(downloadRecord);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // NẾU LƯU LỊCH SỬ BỊ LỖI -> BỎ QUA LUÔN, KHÔNG ĐỂ NÓ CHẶN QUÁ TRÌNH TẢI FILE!
                Console.WriteLine($"[CẢNH BÁO] Lỗi lưu lịch sử tải nhạc: {ex.Message}");
            }

            // 6. Trả kết quả (Vẫn trả về Link cho Frontend tải)
            return new DownloadResponse
            {
                DownloadId = downloadId,
                SongId = song.SongId,
                Title = song.Title,
                FileUrl = bestAudioFile.FileUrl,
                Quality = bestAudioFile.Quality,
                ExpiresAt = null
            };
        }
    }
}