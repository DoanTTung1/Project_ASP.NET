using project_music.DTOs.Songs;
using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.History
{
    // Dùng để Frontend gửi lên khi người dùng nghe xong 1 bài
    public class RecordPlayRequest
    {
        [Required]
        public string SongId { get; set; } = null!;

        [Required]
        public int ListenDurationSeconds { get; set; } // Nghe được bao nhiêu giây rồi mới next?
    }

    // Dùng để trả về danh sách lịch sử cho FE
    public class HistoryResponse
    {
        public string HistoryId { get; set; } = null!;
        public string SongId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime? PlayedAt { get; set; }
        public int ListenDurationSeconds { get; set; }

        // Kéo theo danh sách ca sĩ để hiển thị cho đẹp
        public List<SongArtistResponse> Artists { get; set; } = new List<SongArtistResponse>();
    }
}