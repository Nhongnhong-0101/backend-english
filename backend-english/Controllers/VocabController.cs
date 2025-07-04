using backend_english.Response;
using Core.Models;
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
        private readonly IWSService wSService;
        public VocabController(IVocabService vocabService, IWSService wSService)
        {
            this.vocabService = vocabService;
            this.wSService = wSService;
        }

        [HttpGet("full/{vocab}")]
        public async Task<IActionResult> GetVocabDefinitions(string vocab)
        {
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            vocab = vocab.Trim();
            var defintions = await vocabService.GetFullMeaningsVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<VocabResponse>(200, "Success", defintions));
        }

        [HttpGet("main")]
        public async Task<IActionResult> GetVocabMainDefinition(string vocab)
        {
            if (string.IsNullOrEmpty(vocab))
            {
                return BadRequest(new ApiResponse<string>(400, "Error: Vocab should not be empty", null));
            }
            vocab = vocab.Trim();
            var defintions = await vocabService.GetFullMeaningsVocabAsync(vocab);
            if (defintions == null)
            {
                return NotFound(new ApiResponse<string>(404, $"No definitions found for '{vocab}'", null));
            }
            return Ok(new ApiResponse<VocabResponse>(200, "Success", defintions));
        }

        [HttpPost("save-vocabs/{accountId}")]
        public async Task<IActionResult> SaveVocabsToSavedWordSet(Guid accountId, [FromBody] List<Vocab> vocabs)
        {
            if (accountId == Guid.Empty || vocabs == null || vocabs.Count == 0)
                return BadRequest(new ApiResponse<string>(400, "Invalid account ID or vocabulary data.", null));
            try
            {
                List<VocabWS> vocabWs = new List<VocabWS>();
                foreach (var v in vocabs)
                {
                    VocabWS w = new VocabWS();
                    w.vocab = v.vocab;
                    w.primaryMeaningEn = v.primaryMeaningEn;
                    w.primaryMeaningVi = v.primaryMeaningVi;
                    w.isStar = false;

                    vocabWs.Add(w);
                }

                var success = await wSService.SaveVocabsToSavedWSAsync(vocabWs, accountId);
                return success
                    ? Ok(new ApiResponse<string>(200, "Vocabularies saved to Saved Words successfully.", null))
                    : StatusCode(500, new ApiResponse<string>(500, "Failed to save vocabularies to Saved Words.", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, $"Error saving vocabularies: {ex.Message}", null));
            }
        }

    }
}
