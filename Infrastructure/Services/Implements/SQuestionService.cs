using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Npgsql;
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

        public SQuestionService (ISQuestionRepository repository, IUSResultRepository resultRepository)
        {
            this.repository = repository;
            this.resultRepository = resultRepository;
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
