using Infrastructure.Services.Response;
using Microsoft.AspNetCore.Http;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IChatbotService
    {
        public Task<String> StartConversationAsync(String topic);
        public Task<String> ContinuteConversationAsync(String userInput);
        public Task<String> ReplyUserAudio(IFormFile recored);
        public Task<String> SendToGPTAsync(List<ChatMessage> chatHistory);
        public Task<String> TranscriptAudioAsync(IFormFile recored);
        public void EndConversation();
        public Task<List<KeywordsResponse>> GetKeywordsFromSentenceAsync(List<string> sentence);
        public Task<KeywordsFbResponse> GetKeywordsFeedbackAsync(List<string> keywords, string userSentence, int level);

        public Task <string> FinishChatAndGetReview(List<ChatMessageRecord> chatHistory);

    }
}
