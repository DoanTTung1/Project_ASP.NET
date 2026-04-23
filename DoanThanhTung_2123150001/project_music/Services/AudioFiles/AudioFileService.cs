using Microsoft.EntityFrameworkCore;
using project_music.DTOs.AudioFiles;
using project_music.Models;

namespace project_music.Services.AudioFiles
{
    public class AudioFileService : IAudioFileService
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env; 

        public AudioFileService(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
       
        public async Task<string> SaveCoverAsync(string songId, IFormFile file)
        {
            // 1. Kiểm tra bài hát
            var song = await _context.Songs.FindAsync(songId);
            if (song == null) throw new Exception("Không tìm thấy bài hát để cập nhật ảnh.");

            // 2. Kiểm tra định dạng ảnh
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
                throw new Exception("Hệ thống chỉ hỗ trợ file ảnh định dạng: .jpg, .png, .webp");

            // Giới hạn dung lượng ảnh (Tối đa 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("Ảnh quá lớn. Tối đa 5MB.");

            // 3. Tạo thư mục wwwroot/covers nếu chưa có
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var coversFolder = Path.Combine(webRootPath, "covers");
            if (!Directory.Exists(coversFolder)) Directory.CreateDirectory(coversFolder);

            // 4. Lưu file ảnh (Dùng GUID để không bị trùng tên/lỗi cache)
            var uniqueFileName = $"cover_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(coversFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Cập nhật đường dẫn ảnh vào Database
            var coverUrl = $"/covers/{uniqueFileName}";
            song.CoverUrl = coverUrl;
            await _context.SaveChangesAsync(); // Cập nhật lại cột CoverUrl của bảng Song

            // Trả về link ảnh mới để Frontend hiển thị
            return coverUrl;
        }

        public async Task<AudioFileResponse> UploadAsync(UploadAudioRequest request)
        {
            // 1. Kiểm tra bài hát gốc có tồn tại không
            var song = await _context.Songs.FindAsync(request.SongId);
            if (song == null) throw new Exception("Không tìm thấy bài hát hệ thống.");

            var file = request.File;
            if (file.Length == 0) throw new Exception("File trống.");

            // Giới hạn dung lượng (Ví dụ: 50 MB)
            if (file.Length > 50 * 1024 * 1024) throw new Exception("File quá lớn. Tối đa 50MB.");

            // Kiểm tra đuôi file
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".mp3" && extension != ".wav" && extension != ".flac")
                throw new Exception("Hệ thống chỉ hỗ trợ file .mp3, .wav, .flac");

            // 2. Định tuyến đường dẫn lưu file (Tự động tạo thư mục wwwroot/audios nếu chưa có)
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "audios");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // 3. Đổi tên file để không bị trùng (dùng GUID)
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/audios/{uniqueFileName}"; 
            var newAudio = new AudioFile
            {
                FileId = Guid.NewGuid().ToString(),
                SongId = request.SongId,
                Quality = request.Quality,
                FileUrl = fileUrl,
                SizeBytes = file.Length
            };

            _context.AudioFiles.Add(newAudio);
            await _context.SaveChangesAsync();

            return new AudioFileResponse
            {
                FileId = newAudio.FileId,
                SongId = newAudio.SongId,
                Quality = newAudio.Quality,
                FileUrl = newAudio.FileUrl,
            };
        }
    }
}