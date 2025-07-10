using Infrastructure.Services.Response;

namespace backend_english.Request
{
    public class ReviewChatRequest
    {
        public int level { get; set; }     
        public string topic { get; set; }          
        public List<ChatMessageRecord> chatHistory { get; set; }
    }
}
