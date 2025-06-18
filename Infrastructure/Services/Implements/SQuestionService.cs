using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class SQuestionService : ISQuestionService
    {
        private readonly ISQuestionRepository repository;
        private readonly IUSResultRepository  resultRepository;
        private string gptKey;

        public SQuestionService (ISQuestionRepository repository, IUSResultRepository resultRepository, IConfiguration configuration)
        {
            this.repository = repository;
            this.resultRepository = resultRepository;
            gptKey = configuration["GPT:Chatbot_key"];
        }
        public async Task<List<string>> GetAllTopicsAsync()
        {
            try
            {
                return await repository.GetAllTopicsAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<SpeakingQuestion>> GetByTopicAsync(string topic)
        {
            try
            {
                return await repository.GetByTopicAsync(topic);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            try
            {
                EmbeddingClient client = new(model: "text-embedding-3-small", apiKey: gptKey);
                OpenAIEmbedding  embedding = await client.GenerateEmbeddingAsync(text);
                ReadOnlyMemory<float> vector = embedding.ToFloats();
                return vector.ToArray();
            }
            catch
            {
                throw;
            }
        }

        public Task<float[]> GetEmbeddingAsync(string text, string contentType)
        {
            throw new NotImplementedException();
        }

        public async Task<SpeakingQuestion?> GetFirstQuestionInTopic(string topic)
        {
            try
            {
                var res = await repository.GetFirstQuestionInTopic(topic);
                return res;
            }
            catch
            {
                throw;
            }
        }

        public async Task<SpeakingQuestion?> GetNextQuestionByEmbeddingAsync(float[] userEmbedding, string topic, List<Guid> excludeQuestionIds)
        {
            try
            {
                var question = await repository.GetNextQuestionByEmbeddingAsync(userEmbedding, topic);
                if(question != null && !excludeQuestionIds.Contains(question.questionId) )
                {
                    return question;
                }
                return null;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SpeakingQuestion>> GetPracticeQuestionsAsync(Guid accountId, string topic, int limit = 10)
        {
            try
            {
                //get all questions in topic
                var questions = await repository.GetByTopicAsync(topic.Trim());
                //Get result from table speaking_result of acocunt ID
                var results = await resultRepository.GetResultsOfUserByTopicAsync(accountId, topic.Trim());
                var practicedIds = new HashSet<Guid>();
                //return list questions need practice
                foreach (var r in results)
                {
                    if(r.score >= 80)
                    {
                        practicedIds.Add(r.questionId);
                    }
                }

                var toPractice = new List<SpeakingQuestion>();
                foreach( var q in questions)
                {
                    if(!practicedIds.Contains(q.questionId))
                    {
                        toPractice.Add(q);
                    }

                    if(toPractice.Count >= limit)
                    {
                        break;
                    }
                }
                return toPractice;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ReoderQuestionResponse>> GetReoderQuestionsAsync(string topic, string contentType, int num = 3)
        {
            try
            {
                List<ReoderQuestionResponse> responses = new List<ReoderQuestionResponse>();
                var ques = await repository.GetQuestionsAsync(topic, contentType);
                var random = new Random();

                foreach (var q in ques)
                {
                    var words = q.sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var shuffled = words.OrderBy(_ => random.Next()).ToList();

                    ReoderQuestionResponse r = new ReoderQuestionResponse();

                    r.questionId = q.questionId;
                    r.topic = q.topic;
                    r.correctSentence = q.sentence;
                    r.shuffledWords = shuffled;

                    responses.Add(r);
                }
                return responses;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SpeakingQuestion>> GetQuestionsAsync(string topic, string contentType, int num = 3)
        {
            try
            {
                return await repository.GetQuestionsAsync(topic, contentType, num);
            }
            catch
            {
                throw;
            }
        }


        public async Task<Dictionary<string, TopicProgress>> GetUserTopicProgressAsync(Guid accountId)
        {
            try
            {
                var result = await resultRepository.GetUserResultEachTopicAsync(accountId);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
