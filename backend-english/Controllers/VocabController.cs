using Core;
using Infrastructure.Repository;
using Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabController : ControllerBase
    {
        private readonly IVocabRepository _vocabRepository;
        public VocabController(IVocabRepository vocabRepository)
        {
            _vocabRepository = vocabRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetVocabDefinitions(string vocab)
        {
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            vocab = vocab.Trim();
            var defintions = await _vocabRepository.GetShortMeaningVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<List<Vocab>>(200, "Success", defintions.ToList()));
        }

        [HttpGet]
        public async Task<IActionResult> GetVocabMainDefinition(string vocab)
        {
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            vocab = vocab.Trim();
            var defintions = await _vocabRepository.GetFullMeaningsVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<List<Vocab>>(200, "Success", defintions.ToList()));
        }
    }
}
