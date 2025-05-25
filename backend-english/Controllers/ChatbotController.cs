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
            var response = await chatbotService.SendChatMessageAsync(sentence);
            return Ok(response);
        }

        [HttpPost("transcribe-audio")]
        public async Task<IActionResult> TranscribeAudioAsync(IFormFile audio)
        {
            var text = await chatbotService.TranscriptAudioAsync(audio);
            return Ok(text);
        }

        [HttpPost("speech-to-chat")]
        public async Task<IActionResult> ProcessSpeech([FromForm] IFormFile audio)
        {
            var response = await chatbotService.ProcessSpeechAsync(audio);

            return Ok(response);
        }

    }
}
