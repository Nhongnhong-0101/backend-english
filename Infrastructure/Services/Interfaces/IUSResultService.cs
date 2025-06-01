using Core.Models;
using Infrastructure.Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IUSResultService
    {
        public Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserAsync(Guid userId);
        public Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserByTopicAsync(Guid userId, string topic);
        public Task<IEnumerable<UserSpeakingResult>> SaveResultOfUserAsync(UserSpeakingResult result);
        public Task<Dictionary<string, TopicProgress>> GetUserResultEachTopicAsync(Guid accountId);
    }
}
