using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Genres;     
using project_music.Services.Genres;
using Microsoft.AspNetCore.Authorization;
namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _genreService.GetAllAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _genreService.GetByIdAsync(id);
            return result == null ? NotFound(new { message = "Không tìm thấy thể loại." }) : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGenreRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _genreService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.GenreId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGenreRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _genreService.UpdateAsync(id, request);
                return result == null ? NotFound(new { message = "Không tìm thấy thể loại." }) : Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _genreService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa thành công." }) : NotFound(new { message = "Không tìm thấy." });
        }
    }
}