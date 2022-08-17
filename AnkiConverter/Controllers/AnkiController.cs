using AnkiConverter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnkiConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnkiController : ControllerBase
    {
        private readonly IAnkiService _ankiService;
        public AnkiController(IAnkiService ankiService)
        {
            _ankiService = ankiService;
        }

        [HttpGet, Route("GetDetails/{fileName}")]
        public async Task<IActionResult> GetDetails(string fileName)
        {
            var result = await _ankiService.GetDetailAnki(fileName);

            if (result.Result)
            {
                return Ok(result.Data);
            }

            if (result.Code == 404)
            {
                return NotFound(result.Message);
            }

            return BadRequest(result.Message);
        }
    }
}
