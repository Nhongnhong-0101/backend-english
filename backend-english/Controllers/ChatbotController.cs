using backend_english.Response;
using Infrastructure.Services.Interfaces;
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

        [HttpPost("text-to-chat")]
        public async Task<IActionResult> SendChatMessage ( [FromBody] string sentence)
        {
            try
            {
                var response = await chatbotService.SendChatMessageAsync(sentence);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý ");
            }
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

        [HttpPost("speech-to-chat")]
        public async Task<IActionResult> ProcessSpeech(IFormFile audio)
        {
            try
            {
                var response = await chatbotService.ProcessSpeechAsync(audio);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý chatbot");
            }
        }

    }
}
