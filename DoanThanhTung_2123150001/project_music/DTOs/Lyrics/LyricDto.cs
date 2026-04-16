namespace project_music.DTOs.Lyrics
{
    public class LyricResponse
    {
        public string LyricId { get; set; } = null!;
        public string SongId { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string SyncType { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}