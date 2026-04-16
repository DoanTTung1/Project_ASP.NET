using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.Services.Search;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        [AllowAnonymous] // Ai cũng có thể tìm kiếm
        public async Task<IActionResult> SearchGlobal([FromQuery] string keyword)
        {
            try
            {
                var result = await _searchService.SearchAllAsync(keyword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}