using backend_english.Response;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Reponses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabController : ControllerBase
    {
        private readonly IVocabService vocabService;
        public VocabController(IVocabService vocabService)
        {
            this.vocabService = vocabService;
        }

        [HttpGet("full-definitions/{vocab}")]
        public async Task<IActionResult> GetVocabDefinitions(string vocab)
        {
            vocab = vocab.Trim();
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            var defintions = await vocabService.GetFullMeaningsVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<VocabResponse>(200, "Success", defintions));
        }

        [HttpGet("main-definition/{vocab}")]
        public async Task<IActionResult> GetVocabMainDefinition(string vocab)
        {
            vocab = vocab.Trim();
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            var defintions = await vocabService.GetFullMeaningsVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<VocabResponse>(200, "Success", defintions));
        }
    }
}
