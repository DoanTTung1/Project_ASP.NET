using Microsoft.AspNetCore.Http; 
using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.AudioFiles
{
    public class AudioFileResponse
    {
        public string FileId { get; set; } = null!;
        public string SongId { get; set; } = null!;
        public string Quality { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public long SizeBytes { get; set; }
    }

    public class UploadAudioRequest
    {
        [Required(ErrorMessage = "Mã bài hát là bắt buộc")]
        public string SongId { get; set; } = null!;

        [Required(ErrorMessage = "Chất lượng là bắt buộc (128KBPS, 320KBPS, LOSSLESS)")]
        public string Quality { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn file âm thanh")]
        public IFormFile File { get; set; } = null!; 
    }
}