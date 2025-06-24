using backend_english.Request;
using backend_english.Response;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase 
    {

        private readonly IChatbotService chatbotService;

        public ChatbotController (IChatbotService chatbotService)
        {
            this.chatbotService = chatbotService;
        }

        [HttpPost("transcribe-audio")]
        public async Task<IActionResult> TranscribeAudioAsync(IFormFile audio)
        {
            var tex1t = await chatbotService.TranscriptAudioAsync(audio);

            if (audio == null || audio.Length == 0)
            {
                return BadRequest(new ApiResponse<string>(400, "File ghi âm rỗng.", null));
            }
            if (audio.Length > 25 * 1024 * 1024)//25mb
            {  
                return StatusCode(StatusCodes.Status413PayloadTooLarge, "File ghi âm vượt quá giới hạn 25MB.");
            }
            try
            {
                var text = await chatbotService.TranscriptAudioAsync(audio);
                return Ok(text);
            }
            catch (Exception ex) {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý file ghi âm." + ex.Message);
            }
        }   


        [HttpPost("start-conversation")]
        public async Task<IActionResult> StartConversation([FromForm] String topic)
        {
            try
            {
                var initialQuestion = await chatbotService.StartConversationAsync(topic);

                return Ok(initialQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý chatbot");
            }
        }

        [HttpPost("continue-conversation")]
        public async Task<IActionResult> PracticeInTopic(IFormFile audio)
        {
            try
            {
                var response = await chatbotService.ReplyUserAudio(audio);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý chatbot");
            }
        }

        [HttpPost("end-conversation")]
        public Task<IActionResult> EndConversation(IFormFile audio)
        {
            try
            {
                chatbotService.EndConversation();

                return Task.FromResult<IActionResult>(
                         Ok(new ApiResponse<string>(200, "End the conversation", "")));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(
                        StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý chatbot"));
            }
        }

        [HttpPost("keywords-feedback")]
        public async Task<IActionResult> GetKeywordsFeedback([FromBody] KeywordsFbRequest request)
        {
            try
            {
                if (request.keywords == null || request.keywords.Count == 0 || string.IsNullOrWhiteSpace(request.userSentence))
                {
                    return BadRequest("Keywords and sentence must not be empty.");
                }
                var response = await chatbotService.GetKeywordsFeedbackAsync(request.keywords, request.userSentence);
                return Ok(new ApiResponse<KeywordsFbResponse>(200, null, response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetKeywordsFeedback: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the feedback.");
            }
        }
    }
}
