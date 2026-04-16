using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.Services.Home;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet]
        [AllowAnonymous] // Mở cửa tự do
        public async Task<IActionResult> GetHomePage()
        {
            try
            {
                var result = await _homeService.GetHomeDataAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}