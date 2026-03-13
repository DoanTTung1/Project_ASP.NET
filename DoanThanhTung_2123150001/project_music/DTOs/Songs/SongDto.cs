using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Songs
{
    // Class phụ: Chứa thông tin file nhạc rút gọn (ẩn bớt songId cho đỡ lặp)
    public class SongAudioFileResponse
    {
        public string FileId { get; set; } = null!;
        public string Quality { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public long SizeBytes { get; set; }
    }

    public class SongResponse
    {
        public string SongId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int DurationSeconds { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public long? TotalPlays { get; set; }
        public string? AlbumId { get; set; }

        // ĐIỂM MẤU CHỐT: Thêm một danh sách các file nhạc vào đây
        public List<SongAudioFileResponse> AudioFiles { get; set; } = new List<SongAudioFileResponse>();
    }

    public class CreateSongRequest
    {
        [Required(ErrorMessage = "Tên bài hát không được để trống")]
        [MaxLength(200, ErrorMessage = "Tên bài hát không vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Thời lượng bài hát là bắt buộc")]
        [Range(1, 3600, ErrorMessage = "Thời lượng phải từ 1 đến 3600 giây")]
        public int DurationSeconds { get; set; }

        public DateOnly? ReleaseDate { get; set; }
        public string? AlbumId { get; set; }
    }
}