using project_music.DTOs.Playlists;
using project_music.DTOs.Songs;

namespace project_music.DTOs.Home
{
    // Rổ đựng Thể loại nhạc
    public class GenreResponse
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

    // Rổ Bự nhất bao trọn Trang Chủ
    public class HomeResponse
    {
        // Danh sách Nhạc mới ra mắt
        public List<SongResponse> NewReleases { get; set; } = new List<SongResponse>();

        // Danh sách Playlist gợi ý
        public List<PlaylistResponse> FeaturedPlaylists { get; set; } = new List<PlaylistResponse>();

        // Danh sách Thể loại nhạc
        public List<GenreResponse> Genres { get; set; } = new List<GenreResponse>();

        public string? FileUrl { get; set; } // Hứng link mp3
    }
}