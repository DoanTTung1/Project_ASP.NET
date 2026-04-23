using project_music.DTOs.Songs;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // 👉 Bắt buộc phải có để dùng IFormFile

namespace project_music.DTOs.Artists
{
    public class ArtistResponse
    {
        public string ArtistId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public List<SongResponse>? Songs { get; set; }
    }

    // Dùng chung cho cả Create và Update
    public class CreateArtistRequest
    {
        [Required(ErrorMessage = "Tên nghệ sĩ không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên nghệ sĩ không vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        public string? Bio { get; set; }

        // 👉 THAY ĐỔI LỚN: Dùng IFormFile để hứng file từ máy tính
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverFile { get; set; }
    }
}