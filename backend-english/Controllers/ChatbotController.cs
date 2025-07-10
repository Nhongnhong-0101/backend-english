using backend_english.Request;
using backend_english.Response;
using Infrastructure.Services.Implements;
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
        public async Task<IActionResult> GetKeywordsFeedback([FromForm] KeywordsFbRequest request)
        {
            try
            {
                if (request.keywords == null || request.keywords.Count == 0)
                    return BadRequest("Keywords must not be empty.");
                if (request.audio == null || request.audio.Length == 0)
                    return BadRequest("Audio file is required.");

                var userSentence = await chatbotService.TranscriptAudioAsync(request.audio);

                if (String.IsNullOrEmpty(userSentence))
                {
                    return BadRequest(new ApiResponse<string>(400, "Không thể nhận diện được câu nói. Vui lòng nói rõ hơn.", null));
                }

                var response = await chatbotService.GetKeywordsFeedbackAsync(request.keywords, userSentence, request.level);
                return Ok(new ApiResponse<KeywordsFbResponse>(200, null, response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetKeywordsFeedback: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the feedback.");
            }
        }
        [HttpPost("finish-review")]
        public async Task<IActionResult> FinishChatAndReview([FromBody] ReviewChatRequest request)
        {
            if (request.chatHistory == null || !request.chatHistory.Any())
                return BadRequest(new ApiResponse<string>(400, "Lịch sử hội thoại trống.", null));

            try
            {
                string review = await chatbotService.FinishChatAndGetReview(request.topic, request.level, request.chatHistory);
                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "Lỗi khi đánh giá hội thoại: " + ex.Message, null));
            }
        }
    }
}
