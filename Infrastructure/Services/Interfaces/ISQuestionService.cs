using Core.Models;
using Infrastructure.Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface ISQuestionService
    {
        public Task<List<string>> GetAllTopicsAsync();
        public Task<Dictionary<string, TopicProgress>> GetUserTopicProgressAsync(Guid accountId);

        public Task<List<SpeakingQuestion>> GetByTopicAsync(string topic);
        public Task<List<SpeakingQuestion>> GetPracticeQuestionsAsync(Guid accountId, string topic, int limit = 10);

        public Task<SpeakingQuestion?> GetFirstQuestionInTopic (string topic);
        public Task<SpeakingQuestion?> GetNextQuestionByEmbeddingAsync(float[] userEmbedding, string topic, List<Guid> excludeQuestionIds);
        public Task<float[]> GetEmbeddingAsync(string text);

        public Task<List<ReoderQuestionResponse>> GetReoderQuestionsAsync(string topic, string contentType, int num);
        public Task<List<QuestionResponse>> GetQuestionsAsync(string topic, string contentType, int num);
        public Task<List<SpeakingQuestion>> GetSentenceQuestionAsync(string topic, string contentType, int num);//check
        public Task<List<DialogueQuestionResponse>> GetDialogQuestionAsync(string topic, string contentType, int num);

    }
}
