using Core.Models;
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
    }
}
