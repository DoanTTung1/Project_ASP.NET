using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Songs
{
    public class SongAudioFileResponse
    {
        public string FileId { get; set; } = null!;
        public string Quality { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public long SizeBytes { get; set; }
    }
    public class SongArtistResponse
    {
        public string ArtistId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
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
        public List<SongArtistResponse> Artists { get; set; }= new List<SongArtistResponse>();
    }
    public class SongArtistRequest
    {
        [Required(ErrorMessage ="Mã ca sĩ không được để trống")]
        public string ArtistId { get; set; } = null!;
        public string Role { get; set; } = "MAIN";
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
        [Required(ErrorMessage = "Phải có ít nhất một ca sĩ cho bài hát")]
        public List<SongArtistRequest> Artists { get; set; } = new List<SongArtistRequest>();
    }
}