using Core.Models;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface ISQuestionRepository
    {
        Task<List<SpeakingQuestion>> GetAllAsync();
        Task<List<string>> GetAllTopicsAsync();
        Task<List<SpeakingQuestion>> GetByTopicAsync(string topic);
        Task<SpeakingQuestion?> GetByIdAsync(Guid questionId);
        Task<SpeakingQuestion?> GetFirstQuestionInTopic( string topic );
        Task<SpeakingQuestion?> GetNextQuestionByEmbeddingAsync(float[] userAnswerEmbedding, string topic);
        Task<List<SpeakingQuestion>> GetQuestionsAsync(string topic, string contentType, int num = 3 );

        Task<SpeakingQuestion> UpdateQuesionAsync(SpeakingQuestion question);

        Task<List<SpeakingQuestion>> FindByTopicVectorAsync(Vector topicEmbedding, int limit);

    }
}
