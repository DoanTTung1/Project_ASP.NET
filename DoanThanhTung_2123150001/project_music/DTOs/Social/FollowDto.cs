namespace project_music.DTOs.Social
{
    // Dùng để trả về danh sách ca sĩ mà User đang theo dõi
    public class FollowedArtistResponse
    {
        public string ArtistId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime? FollowedAt { get; set; }
    }
}