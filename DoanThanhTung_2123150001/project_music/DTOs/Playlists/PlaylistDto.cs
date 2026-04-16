using project_music.DTOs.Songs;
using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Playlists
{
    public class CreatePlaylistRequest
    {
        [Required(ErrorMessage = "Tên Playlist không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên Playlist không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public bool IsPublic { get; set; } = true;
    }
    public class PlaylistResponse
    {
        public string PlaylistId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string CreatorName { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
        public string? CoverUrl { get; set; }
        public int TotalSongs { get; set; }


    }
    public class AddSongRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn bài hát")]
        public string SongId { get; set; } = null!;

    }

    public class PlaylistDetailResponse : PlaylistResponse
    {
        // Chứa danh sách các bài hát nằm trong Playlist này
        public List<SongResponse> Songs { get; set; } = new List<SongResponse>();
    }

}