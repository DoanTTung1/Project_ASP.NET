using project_music.DTOs.Artists;

namespace project_music.Services.Artists
{
    public interface IArtistService
    {
        Task<List<ArtistResponse>> GetAllArtistsAsync();
        Task<ArtistResponse?> GetArtistByIdAsync(string artistId);
        Task<ArtistResponse> CreateArtistAsync(CreateArtistRequest request);
        Task<bool> DeleteArtistAsync(string id);
        Task<ArtistResponse> UpdateArtistAsync(string id, CreateArtistRequest request);
    }
}