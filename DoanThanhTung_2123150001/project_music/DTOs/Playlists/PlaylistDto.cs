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

}