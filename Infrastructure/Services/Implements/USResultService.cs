using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class USResultService : IUSResultService
    {
        private readonly IUSResultRepository uSResultRepository;

        public USResultService(IUSResultRepository uSResultRepository)
        {
            this.uSResultRepository = uSResultRepository;
        }

        public async Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserAsync(Guid userId)
        {
            try
            {
                return await uSResultRepository.GetResultsOfUserAsync(userId);
            }
            catch
            {
                throw; 
            }
        }

        public async Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserByTopicAsync(Guid userId, string topic)
        {
            try
            {
                return await uSResultRepository.GetResultsOfUserByTopicAsync(userId, topic);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, (int total, int practiced)>> GetUserResultEachTopicAsync(Guid accountId)
        {
            try
            {
                return await uSResultRepository.GetUserResultEachTopicAsync(accountId);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<UserSpeakingResult>> SaveResultOfUserAsync(UserSpeakingResult result)
        {
            try
            {
                return await uSResultRepository.SaveResultOfUserAsync(result);
            }
            catch
            {
                throw;
            }
        }
    }
}
