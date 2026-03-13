using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Genres
{
    // 1. Dữ liệu trả về cho người dùng xem
    public class GenreResponse
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

    // 2. Dữ liệu nhận vào khi Thêm mới (POST)
    public class CreateGenreRequest
    {
        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [MaxLength(50, ErrorMessage = "Tên thể loại không vượt quá 50 ký tự")]
        public string Name { get; set; } = null!;

        public string? ImageUrl { get; set; }
    }

    // 3. Dữ liệu nhận vào khi Cập nhật (PUT)
    public class UpdateGenreRequest
    {
        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [MaxLength(50, ErrorMessage = "Tên thể loại không vượt quá 50 ký tự")]
        public string Name { get; set; } = null!;

        public string? ImageUrl { get; set; }
    }
}