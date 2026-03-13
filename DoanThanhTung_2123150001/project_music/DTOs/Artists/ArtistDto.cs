using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Artists
{
    public class ArtistResponse
    {
        public string ArtistId { get; set; } = null;
        public string Name { get; set; } = null;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
    }
    public class CreateArtistRequest
    {
        [Required(ErrorMessage = "Tên nghệ sĩ không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên nghệ sĩ không vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
    }
}
