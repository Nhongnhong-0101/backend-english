using backend_english.Response;
using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
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
        private readonly IChatbotService chatbotService;

        public SQuestionController(ISQuestionService questionService, IChatbotService chatbotService)
        {
            this.questionService = questionService;
            this.chatbotService = chatbotService;
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetAllTopicsWithProgress([FromQuery] Guid accountId)
        {
            var result = await questionService.GetUserTopicProgressAsync(accountId);
            return Ok(new ApiResponse<Dictionary<string, TopicProgress>>(200, null, result));
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


        [HttpGet("reoder-question")]
        public async Task<IActionResult> GetReoderQuestions([FromQuery] string topic, [FromQuery] string contentType, int num)
        {
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(contentType))
            {
                return BadRequest("Topic and ContentType are required.");
            }

            var questions = await questionService.GetReoderQuestionsAsync(topic, contentType, num);

            if (questions == null || questions.Count == 0)
            {
                return NotFound("No questions found.");
            }

            return Ok(questions);
        }

        [HttpGet("keyword-question")]
        public async Task<IActionResult> GetKeyWordQuestion([FromQuery] string topic, [FromQuery] string contentType, int num =3)
        {
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(contentType))
            {
                return BadRequest("Topic and ContentType are required.");
            }

            List<SpeakingQuestion> ques = await questionService.GetQuestionsAsync(topic, contentType, num);

            List<string> sententces = new List<string>();
            foreach (var que in ques)
            {
                sententces.Add(que.sentence);
            }

            List<KeywordsResponse> keywords = new List<KeywordsResponse>();
            if (sententces.Count > 0)
            {
                keywords = await chatbotService.GetKeywordsFromSentenceAsync(sententces);
            }

            if (keywords == null || keywords.Count == 0)
            {
                return NotFound("No questions found.");
            }

            return Ok(keywords);
        }
        public class SaveSpeakingResultRequest
        {
            public List<UserSpeakingResult> Results { get; set; } = new();
        }
    }
}
