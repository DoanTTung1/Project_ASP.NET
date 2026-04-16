using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Downloads
{
    // Dữ liệu Front-end gửi lên khi bấm nút "Tải xuống"
    public class DownloadRequest
    {
        [Required(ErrorMessage = "Vui lòng truyền ID bài hát")]
        public string SongId { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng truyền ID thiết bị (Device ID)")]
        public string DeviceId { get; set; } = null!;
    }

    // Trả về link file gốc để Front-end tải về máy
    public class DownloadResponse
    {
        public string DownloadId { get; set; } = null!;
        public string SongId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string Quality { get; set; } = null!;

        // Hạn sử dụng của file tải về (Bằng đúng ngày hết hạn VIP)
        public DateTime? ExpiresAt { get; set; }
    }
}