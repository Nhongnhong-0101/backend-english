using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IrregularVerbController : ControllerBase
    {
        private readonly IVerbService _verbService;
        public IrregularVerbController(IVerbService verbService)
        {
            _verbService = verbService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllIrregularVerbs()
        {
            var data = await _verbService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<IrregularVerb>>(200, "Lấy danh sách idioms thành công", data));
        }
    }
}
