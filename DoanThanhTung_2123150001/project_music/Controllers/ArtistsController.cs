using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Artists;
using project_music.Services.Artists;
using Microsoft.AspNetCore.Authorization;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistService _artistService;

        public ArtistsController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        // GET: api/Artists
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return Ok(artists);
        }

        // GET: api/Artists/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound(new { message = "Không tìm thấy nghệ sĩ này." });
            }
            return Ok(artist);
        }

        // POST: api/Artists
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateArtistRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdArtist = await _artistService.CreateArtistAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = createdArtist.ArtistId }, createdArtist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 👉 THÊM MỚI: PUT: api/Artists/{id} (ĐỂ SỬA NGHỆ SĨ)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] CreateArtistRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Dùng chung CreateArtistRequest hoặc Boss có thể tạo UpdateArtistRequest riêng
                var updatedArtist = await _artistService.UpdateArtistAsync(id, request);
                return Ok(updatedArtist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 👉 THÊM MỚI: DELETE: api/Artists/{id} (ĐỂ XÓA NGHỆ SĨ)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _artistService.DeleteArtistAsync(id);
                if (result) return Ok(new { message = "Xóa nghệ sĩ thành công!" });
                return BadRequest(new { message = "Không thể xóa nghệ sĩ này." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}