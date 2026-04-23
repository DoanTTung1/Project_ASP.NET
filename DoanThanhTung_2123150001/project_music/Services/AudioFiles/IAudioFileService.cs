using project_music.DTOs.AudioFiles;

namespace project_music.Services.AudioFiles
{
    public interface IAudioFileService
    {
        Task<AudioFileResponse> UploadAsync(UploadAudioRequest request);
        Task<string> SaveCoverAsync(string songId, IFormFile file);
    }
}