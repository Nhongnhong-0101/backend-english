using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface ILessonRepository
    {
        public Task<IEnumerable<Lesson>> GetLessonsOfAccount(Guid accountId, Guid planId);
        public Task<bool> UpdateAccountPassLesson(Guid accountId, Guid lessonId);
        public Task<Lesson?> GetLessonDetail(Guid lessonId);
    }
}
