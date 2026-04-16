using project_music.DTOs.Search;

namespace project_music.Services.Search
{
    public interface ISearchService
    {
        // Hàm tìm kiếm đa năng theo từ khóa
        Task<SearchResponse> SearchAllAsync(string keyword);
    }
}