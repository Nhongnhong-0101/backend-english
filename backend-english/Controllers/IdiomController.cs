using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdiomController : ControllerBase
    {
        private readonly IIdiomService idiomService;

        public IdiomController(IIdiomService idiomService)
        {
            this.idiomService = idiomService;
        }

        [HttpGet("idioms")]
        public async Task<IActionResult> GetAllIdioms()
        {
            try
            {
                var idioms = await idiomService.GetAllIdiom();
                return Ok(new ApiResponse<IEnumerable<Idiom>>(200, "Lấy danh sách idioms thành công", idioms));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "Lỗi server", ex.Message));
            }
        }

        [HttpPost("idioms")]
        public async Task<IActionResult> AddNewIdiom([FromBody] Idiom newIdiom)
        {
            if (string.IsNullOrWhiteSpace(newIdiom.idiom) || string.IsNullOrWhiteSpace(newIdiom.meaning))
            {
                return BadRequest(new ApiResponse<string>(400, "Idiom và meaning không được để trống", null));
            }

            try
            {
                var addedIdiom = await idiomService.AddNewIdiom(newIdiom);
                if (addedIdiom != null)
                {
                    return Ok(new ApiResponse<Idiom>(200, "Thêm idiom thành công", addedIdiom));
                }
                else
                {
                    return StatusCode(500, new ApiResponse<string>(500, "Không thể thêm idiom", null));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "Lỗi server", ex.Message));
            }
        }
       
        
    }
}
