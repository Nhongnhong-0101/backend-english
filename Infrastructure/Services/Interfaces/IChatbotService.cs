using Infrastructure.Services.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IChatbotService
    {
        public Task<string> SendChatMessageAsync(string message);
        public Task<string> TranscriptAudioAsync(IFormFile recored);

        public Task<ChatResponse> ProcessSpeechAsync(IFormFile recored);


    }
}
