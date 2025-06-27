using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository lessonRepository;
        private readonly ISQuestionRepository questionRepository;
        public LessonService(ILessonRepository lessonRepository, ISQuestionRepository questionRepository)
        {
            this.lessonRepository = lessonRepository;
            this.questionRepository = questionRepository;
        }

        public Task<IEnumerable<Lesson>> GetLessonsOfAccount(Guid accountId, Guid planId)
        {
            try
            {
                return lessonRepository.GetLessonsOfAccount(accountId, planId);
            }
            catch
            {
                throw;
            }
        }
        public Task<bool> UpdateAccountPassLesson(Guid accountId, Guid lessonId)
        {
            try
            {
                return lessonRepository.UpdateAccountPassLesson(accountId, lessonId);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<SpeakingQuestion>> GetQuestionsOfLesson(Guid idLesson)
        {
            List<SpeakingQuestion> questionList = new List<SpeakingQuestion>();
            try
            {
                Lesson lesson = await lessonRepository.GetLessonDetail(idLesson);

                if (lesson != null)
                {
                    if (!String.IsNullOrEmpty(lesson.example))
                    {
                        var words = lesson.example
                           .Split(new[] { '.', ',', ';', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(w => w.Trim().ToLower())
                           .Distinct()
                           .ToList();
                        if (words.Any())
                        {
                            foreach(var w in  words)
                            {
                                SpeakingQuestion question = new SpeakingQuestion();
                                question.questionId = Guid.NewGuid();
                                question.contentType = "repeat";
                                question.sentence = w;
                                question.topic = "";
                                question.level = "easy";

                                questionList.Add(question);
                            }
                        }
                    }
                    else if (!String.IsNullOrEmpty(lesson.topics))
                    {
                        //get speakibg question with each topic in db 
                        var topics = lesson.topics
                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim().ToLower())
                            .Distinct()
                            .ToList();
                        if(topics.Any())
                        {
                            foreach( var topic in topics)
                            {
                                var questions = await questionRepository.GetByTopicAsync(topic);
                                questionList.AddRange(questions);
                            }
                        }
                    }
                }
                return questionList;
            }
            catch
            {
                throw;
            }
        }

        
    }
}
