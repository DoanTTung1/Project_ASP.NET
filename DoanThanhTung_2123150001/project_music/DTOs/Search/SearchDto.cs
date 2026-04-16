namespace project_music.DTOs.Search
{
    // Rổ chứa kết quả tổng hợp
    public class SearchResponse
    {
        public List<SearchSongResponse> Songs { get; set; } = new List<SearchSongResponse>();
        public List<SearchArtistResponse> Artists { get; set; } = new List<SearchArtistResponse>();
        public List<SearchPlaylistResponse> Playlists { get; set; } = new List<SearchPlaylistResponse>();
    }

    // Các class con để hứng dữ liệu cho nhẹ (không cần lấy quá nhiều chi tiết khi tìm kiếm)
    public class SearchSongResponse
    {
        public string SongId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? ArtistName { get; set; } // Tên ca sĩ hát chính
        public string? CoverUrl { get; set; } // Lấy từ Album nếu có
    }

    public class SearchArtistResponse
    {
        public string ArtistId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class SearchPlaylistResponse
    {
        public string PlaylistId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public string CreatorName { get; set; } = null!;
    }
}