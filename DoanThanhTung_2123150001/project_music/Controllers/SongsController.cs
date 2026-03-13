using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Songs;
using project_music.Services.Songs;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _songService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _songService.GetByIdAsync(id);
            return result == null ? NotFound(new { message = "Không tìm thấy bài hát." }) : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSongRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _songService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.SongId }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _songService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa bài hát thành công (Xóa mềm)." }) : NotFound(new { message = "Không tìm thấy." });
        }
    }
}