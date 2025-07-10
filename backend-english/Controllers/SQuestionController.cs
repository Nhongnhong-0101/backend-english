using backend_english.Response;
using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Pgvector;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SQuestionController : ControllerBase
    {
        private readonly ISQuestionService questionService;
        private readonly IUSResultService resultService;
        private readonly IChatbotService chatbotService;

        public SQuestionController(ISQuestionService questionService, IChatbotService chatbotService)
        {
            this.questionService = questionService;
            this.chatbotService = chatbotService;
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetAllTopicsWithProgress([FromQuery] Guid accountId)
        {
            var result = await questionService.GetUserTopicProgressAsync(accountId);
            return Ok(new ApiResponse<Dictionary<string, TopicProgress>>(200, null, result));
        }

        [HttpGet("topic-questions")]
        public async Task<IActionResult> GetQuestionsByTopic([FromQuery] string topic)
        {
            var result = await questionService.GetByTopicAsync(topic);
            return Ok(new ApiResponse<IEnumerable<SpeakingQuestion>>(200, null, result));
        }

        [HttpGet("practice")]
        public async Task<IActionResult> GetPracticeQuestions([FromQuery] Guid accountId, [FromQuery] string topic, [FromQuery] int limit = 10)
        {
            var result = await questionService.GetPracticeQuestionsAsync(accountId, topic, limit);
            return Ok(new ApiResponse<IEnumerable<SpeakingQuestion>>(200, null, result));
        }

        [HttpPost("save-result")]
        public async Task<IActionResult> SaveSpeakingResult([FromBody] SaveSpeakingResultRequest request)
        {
            if (request.Results == null || !request.Results.Any())
            {
                return BadRequest(new { message = "No result provided." });
            }

            foreach (var result in request.Results)
            {
                await resultService.SaveResultOfUserAsync(result);
            }

            return Ok(new { message = "Saved successfully" });
        }


        [HttpGet("keyword-question")]
        public async Task<IActionResult> GetKeyWordQuestion([FromQuery] string topic, [FromQuery] string contentType = "sentence", int num =3)
        {
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(contentType))
            {
                return BadRequest("Topic and ContentType are required.");
            }

            //List<SpeakingQuestion> ques = await questionService.GetQuestionsAsync(topic, contentType, num);

            //List<string> sententces = new List<string>();
            //foreach (var que in ques)
            //{
            //    sententces.Add(que.sentence);
            //}

            //List<KeywordsResponse> keywords = new List<KeywordsResponse>();
            //if (sententces.Count > 0)
            //{
            //    keywords = await chatbotService.GetKeywordsFromSentenceAsync(sententces);
            //}

            //if (keywords == null || keywords.Count == 0)
            //{
            //    return NotFound("No questions found.");
            //}

            var fakeData = new List<QuestionResponse>
            {
                new QuestionResponse
                {
                    type = "reoder",
                    instructions = "Arrange the words in correct order.",
                    data = new KeywordsResponse
                    {
                        sentence = "I am currently a student.",
                        keywords = new List<string> { "currently", "student", "I" }
                    },

                },
                 new QuestionResponse
                {
                    type = "reoder",
                    instructions = "Arrange the words in correct order.",
                    data = new KeywordsResponse
                    {
                        sentence = "He is playing football.",
                        keywords = new List<string> { "he", "football", "playing" }
                    },

                },
                  new QuestionResponse
                {
                    type = "reoder",
                    instructions = "Arrange the words in correct order.",
                    data = new KeywordsResponse
                    {
                        sentence = "She likes reading books at night.",
                        keywords = new List<string> { "reading", "books", "she", "night" }
                    },

                },
              
            };
            //return Ok(keywords);
            return Ok(new ApiResponse<List<QuestionResponse>>(200, null, fakeData));
        }
   
        [HttpGet("random-question")]
        public async Task<IActionResult> GetQuestions([FromQuery] string topic, [FromQuery] string contentType = "dialogue", int num = 3)
        {
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(contentType))
            {
                return BadRequest("Topic and ContentType are required.");
            }
            if (contentType == "keyword")
            {
                return await GetKeyWordQuestion(topic, "sentence", num);
            }
            else
            {
                var ques = await questionService.GetQuestionsAsync(topic, contentType, num);
                return Ok(new ApiResponse<List<QuestionResponse>>(200, null, ques));
            }

            //var fakeData = new List<QuestionResponse>
            //{
            //    new QuestionResponse
            //    {
            //        type = "keyword",
            //        instructions = "Arrange the words in correct order.",
            //        data = new KeywordsResponse
            //        {
            //            sentence = "I am currently a student.",
            //            keywords = new List<string> { "currently", "student", "I" }
            //        },

            //    },
            //     new QuestionResponse
            //    {
            //        type = "sentence",
            //        instructions = "Speak clerly.",
            //        data = new 
            //        {
            //            sentence = "He is playing football.",
            //        },

            //    },
            //      new QuestionResponse
            //    {
            //        type = "word",
            //        instructions = "speak clearly",
            //        data = new 
            //        {
            //            sentence = "night",
            //        },

            //    },
            //      new QuestionResponse
            //    {
            //        type = "reoder",
            //        instructions = "Arrange these words to make a right sentence",
            //        data = new ReoderQuestionResponse
            //        {
            //            sentence = "I am currently a student.",
            //            shuffledWords = ["I", "currently", "student", "am", "a"]
            //        },

            //    },
            //      new QuestionResponse
            //    {
            //        type = "prompt",
            //        instructions = "Let talk about this topic",
            //        data = new 
            //        {
            //            sentence = "Let's talk about yourself",
            //        },

            //    },

            //};
            //return Ok(keywords);
            //return Ok(new ApiResponse<List<QuestionResponse>>(200, null, fakeData));


        }

        [HttpPost("update-embedding")]
        public async Task<IActionResult> UpdateEmbedding()
        {
            try
            {
                var questionIds = new List<Guid>
                {
                    Guid.Parse("8cb4fa1f-e6a7-4e21-b7bc-b46a737dc87e"),
                    Guid.Parse("969eb735-c877-4d6f-9c88-299ff7dd4c63"),
                    Guid.Parse("9fa2c7ab-4bcc-4ff8-be86-6dcf3d10c908"),
                    Guid.Parse("aae1c9d8-5f5a-468c-af96-c3b8e9bde8ef"),
                    Guid.Parse("ab9d2eb7-daf2-4e89-995d-5f968b77d2e0"),
                    Guid.Parse("b222266d-1e05-4d96-959e-c569cb123ccd"),
                    Guid.Parse("bc74e50d-3647-4f9e-be72-590824988e4b"),
                    Guid.Parse("d55e69ce-aac7-46c7-8a8a-0d91655cbd72"),
                    Guid.Parse("ddc8c03f-e7ff-4d89-a723-b6fcfcd0ee8e"),
                    Guid.Parse("f7b45996-5058-400c-9707-c360e3b9cc2c")
                };
                foreach(Guid idQuestion in questionIds)
                {
                    var success = await questionService.GetByIdAsync(idQuestion);

                    if (success.sentence == null)
                        return NotFound(new { message = "Không tìm thấy câu hỏi hoặc lỗi khi tạo embedding." });
                    var vector = await chatbotService.CreateEmbeddingAsync(success.sentence);
                    success.embedding = new Pgvector.Vector(vector);

                    var updated = await questionService.UpdateQuestionAsync(success);

                }
                    return Ok(new { message = "Cập nhật embedding thành công." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("questions-by-topic-embedding")]
        public async Task<IActionResult> GetQuestionsByTopic([FromQuery] string topic, [FromQuery] int topK = 5)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return BadRequest("Topic must not be empty.");

            try
            {
                var vector = await chatbotService.CreateEmbeddingAsync(topic);
                Vector topicVector = new Vector(vector);

                var data = await questionService.GetQuestionsNearTopic(topicVector, topK);

                return Ok(new ApiResponse<List<QuestionResponse>>(200, null, data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }
        public class SaveSpeakingResultRequest
        {
            public List<UserSpeakingResult> Results { get; set; } = new();
        }
    }
}
