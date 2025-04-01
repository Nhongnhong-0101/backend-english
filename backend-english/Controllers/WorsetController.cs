using Core.Models;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorsetController : ControllerBase
    {
        private readonly IWSService wSService;

        public WorsetController(IWSService wSService)
        {
            this.wSService = wSService;
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetWordSetsByAccount(Guid accountId)
        {
            try
            {
                if (accountId == Guid.Empty)
                    return BadRequest(new ApiResponse<string>(400, "Errorrr: Please check account ID", null));

                var wordSets = await wSService.GetWordSetsOfAccountAsync(accountId);
                return Ok(new ApiResponse<IEnumerable<WordSet>>(200, null, wordSets));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWordSetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new ApiResponse<string>(400, "Invalid WordSet ID.", null));

                var wordSet = await wSService.GetWordSetByIdAsync(id);
                if (wordSet == null)
                    return NotFound(new ApiResponse<string>(404, "WordSet not found.", null));

                return Ok(new ApiResponse<WordSet>(200, null, wordSet));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddWordSet([FromBody] WordSet wordSet)
        {
            try
            {
                if (wordSet == null || string.IsNullOrWhiteSpace(wordSet.nameSet))
                    return BadRequest(new ApiResponse<string>(400, "Invalid WordSet data.", null));

                var newWordSet = await wSService.AddNewWordSetAsync(wordSet);
                return Ok(new ApiResponse<WordSet?>(200, null, newWordSet));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWordSet(Guid id, [FromBody] WordSet wordSet)
        {
            try
            {
                if (wordSet == null || string.IsNullOrWhiteSpace(wordSet.nameSet))
                    return BadRequest(new ApiResponse<string>(400, "Invalid WordSet data.", null));

                var updateWS = await wSService.UpdateWordSetAsync(wordSet);
                if (updateWS == null)
                    return NotFound(new ApiResponse<string>(404, "WordSet not found.", null));
                return Ok(new ApiResponse<WordSet>(200, null, updateWS));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWordSet(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new ApiResponse<string>(400, "Invalid WordSet data.", null));

            try
            {
                await wSService.DeleteWordSetByIdAsync(id);
                return Ok(new ApiResponse<string>(200, "Delete successfully", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errorr deleting WordSet: {ex.Message}");
            }
        }

        [HttpGet("{wsId}/vocabs")]
        public async Task<IActionResult> GetVocabsOfWordSet(Guid wsId)
        {
            if (wsId == Guid.Empty)
                return BadRequest(new ApiResponse<string>(400, "Invalid WordSet ID.", null));

            try
            {
                var vocabs = await wSService.GetVocabsOfWSAsync(wsId);
                return Ok(new ApiResponse<IEnumerable<Vocab>>(200, null, vocabs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errorr retrieving vocabs: {ex.Message}");
            }
        }

        [HttpPost("{wsId}/vocabs")]
        public async Task<IActionResult> AddVocabsToWordSet(Guid wsId, [FromBody] List<Vocab> vocabs)
        {
            if (wsId == Guid.Empty || vocabs == null || vocabs.Count == 0)
                return BadRequest(new ApiResponse<string>(400, "Invalid WordSet data.", null));
            try
            {
                var success = await wSService.AddVocabsToWSAsync(vocabs, wsId);
                return success ? Ok(new ApiResponse<string>(200, "Success", null)) : StatusCode(500, "Failed to add vocabs.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errorr adding vocabs: {ex.Message}");
            }
        }
    }
}
