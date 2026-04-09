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
            return Ok(artists); // 200 OK
        }

        // GET: api/Artists/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound(new { message = "Không tìm thấy nghệ sĩ này." }); // 404 Not Found
            }
            return Ok(artist); // 200 OK
        }

        // POST: api/Artists
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArtistRequest request)
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
    }
}