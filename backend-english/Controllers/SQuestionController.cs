using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SQuestionController : ControllerBase
    {
        private readonly ISQuestionService questionService;
        private readonly IUSResultService resultService;

        public SQuestionController(ISQuestionService questionService)
        {
            this.questionService = questionService;
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetAllTopicsWithProgress([FromQuery] Guid accountId)
        {
            var result = await questionService.GetUserTopicProgressAsync(accountId);
            return Ok(new ApiResponse<Dictionary<string, (int, int)>>(200, null, result));
        }

        [HttpGet("topic-questions")]
        public async Task<IActionResult> GetQuestionsByTopic([FromQuery] string topic)
        {
            var result = await questionService.GetByTopicAsync(topic);
            return Ok(new ApiResponse<IEnumerable<SpeakingQuestion>>(200, null, result));
        }

        [HttpGet("practice")]
        public async Task<IActionResult> GetPracticeQuestions([FromQuery] Guid accountId, [FromQuery] string topic, [FromQuery] int limit = 10)
        {
            var result = await questionService.GetPracticeQuestionsAsync(accountId, topic, limit);
            return Ok(new ApiResponse<IEnumerable<SpeakingQuestion>>(200, null, result));
        }

        [HttpPost("save-result")]
        public async Task<IActionResult> SaveSpeakingResult([FromBody] SaveSpeakingResultRequest request)
        {
            if (request.Results == null || !request.Results.Any())
            {
                return BadRequest(new { message = "No result provided." });
            }

            foreach (var result in request.Results)
            {
                await resultService.SaveResultOfUserAsync(result);
            }

            return Ok(new { message = "Saved successfully" });
        }

        public class SaveSpeakingResultRequest
        {
            public List<UserSpeakingResult> Results { get; set; } = new();
        }
    }
}
