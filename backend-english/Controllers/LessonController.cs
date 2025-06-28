using backend_english.Request;
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
    public class LessonController : ControllerBase
    {
        private readonly ILessonService lessonService;
        public LessonController(ILessonService lessonService)
        {
            this.lessonService = lessonService;
        }
        [HttpGet("lessons")]
        public async Task<IActionResult> GetLesson(Guid accountId, Guid planId)
        {
            var lessons = await lessonService.GetLessonsOfAccount(accountId, planId);
            return Ok(new ApiResponse<IEnumerable<Lesson>>(200, null, lessons));
        }

        [HttpPost("complete")]
        public async Task<IActionResult> MarkPlanAsCompleted([FromBody] CompleteLessonRequest request)
        {
            if (request == null || request.accountId == Guid.Empty || request.lessonId == Guid.Empty)
            {
                return BadRequest("Invalid data.");
            }

            var result = await lessonService.UpdateAccountPassLesson(request.accountId, request.lessonId);

            if (result)
                return Ok(new { message = "Lesson marked as completed." });
            else
                return StatusCode(500, "Failed to update progress.");
        }

        [HttpGet("{lessonId}/speaking-questions")]
        public async Task<IActionResult> GetSpeakingQuestionsForLesson(Guid lessonId, int limit= 10)
        {
            try
            {
                var questions = await lessonService.GetQuestionsOfLesson(lessonId, limit);
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success",
                    Data = questions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = $"Internal server error: {ex.Message}",
                    Data = (object?)null
                });
            }
        }
    }
}
