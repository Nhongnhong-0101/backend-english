using Core.Models;
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
        public Task<Dictionary<string, (int, int)>> GetUserTopicProgressAsync(Guid accountId);

        public Task<List<SpeakingQuestion>> GetByTopicAsync(string topic);
        public Task<List<SpeakingQuestion>> GetPracticeQuestionsAsync(Guid accountId, string topic, int limit = 10);

    }
}
