using backend_english.Request;
using backend_english.Response;
using backend_english.Response.Pronounciation;
using Core.Models;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ValuesController : ControllerBase
    {
        private readonly IValueService valueService;

        public ValuesController(IValueService valueService)
        {
            this.valueService = valueService;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckPronunciation([FromForm] PronunciationRequest request)
        {
            if (request.audio == null || string.IsNullOrEmpty(request.sentence))
                return BadRequest(new ApiResponse<string>(400, "Please check input", null));
            else
            {
                return Ok(await valueService.sendToAzure(sententce: request.sentence, recored: request.audio));
                //return Ok(valueService.FakeResponse());
            }
            
        }
        
    }
}
