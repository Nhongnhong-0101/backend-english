using Core.Models;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface ILessonService
    {
        public Task<IEnumerable<Lesson>> GetLessonsOfAccount(Guid accountId, Guid planId);
        public Task<bool> UpdateAccountPassLesson(Guid accountId, Guid lessonId);

        public Task<IEnumerable<SpeakingQuestion>> GetQuestionsOfLesson(Guid idLesson, int limit);

        public Task<List<SpeakingQuestion>> FindByTopicVectorAsync(Vector embeddingTopic, int limit);
    }
}
