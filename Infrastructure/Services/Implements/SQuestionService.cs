using Azure;
using Core.Models;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenAI;
using OpenAI.Embeddings;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
                foreach( var que in ques)
                {
                    var q = parseToReorder(que);
                    responses.Add(q);
                }
                return responses;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<QuestionResponse>> GetQuestionsAsync(string topic, string contentType, int num = 3)
        {
            try
            {
                List<QuestionResponse> responses = new List<QuestionResponse>();
                List<SpeakingQuestion> questions = await repository.GetQuestionsAsync(topic, contentType, num);
                if (!String.IsNullOrEmpty(contentType.Trim()))
                {
                    foreach (var question in questions)
                    {
                        switch (question.contentType)
                        {
                            case "sentence":
                            case "word":
                                QuestionResponse response = new QuestionResponse();
                                response.questionId = question.questionId;
                                response.instructions = "Repeat this word/sentence clearly";
                                response.type = "sentence";
                                response.data = new Dictionary<string, string>
                                {
                                    { "sentence", question.sentence }
                                };
                                responses.Add(response);
                                break;

                            case "dialogue":
                                QuestionResponse r = new QuestionResponse();
                                r.questionId = question.questionId;
                                r.instructions = "Repeat this sentence";
                                r.type = "dialogue";
                                r.data = parseToDialogue(question);
                                responses.Add(r);

                                break;

                            case "reorder":
                                QuestionResponse q = new QuestionResponse();
                                q.questionId = question.questionId;
                                q.instructions = "Repeat this sentence";
                                q.type = "reorder";
                                q.data = parseToReorder(question);
                                responses.Add(q);

                                break;

                            case "prompt":
                                QuestionResponse p = new QuestionResponse();
                                p.questionId = question.questionId;
                                p.instructions = "Let talk about this topic";
                                p.type = "prompt";
                                p.data = new Dictionary<string, string>
                                {
                                    { "sentence", question.sentence }
                                };
                                responses.Add(p);

                                break;

                            default:
                                break; 
                        }
                    }

                }
                return responses;
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

        public Task<List<SpeakingQuestion>> GetSentenceQuestionAsync(string topic, string contentType, int num)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DialogueQuestionResponse>> GetDialogQuestionAsync(string topic, string contentType = "dialogue", int num =3)
        {
            try
            {
                List<DialogueQuestionResponse> responses = new List<DialogueQuestionResponse>();
                var ques = await repository.GetQuestionsAsync(topic, contentType);

                foreach (var q in ques)
                {
                    var dialogue = parseToDialogue(q);
                    responses.Add(dialogue);
                }
                return responses;
            }
            catch
            {
                throw;
            }
        }
        public DialogueQuestionResponse parseToDialogue(SpeakingQuestion q)
        {
            var segments = q.sentence.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            var dialogue = new DialogueQuestionResponse();

            for (int i = 0; i < segments.Count; i++)
            {
                var line = new DialogueLine
                {
                    speaker = i % 2 == 0 ? "A" : "B",
                    sentence = segments[i].Trim()
                };

                dialogue.lines.Add(line);
            }
            return dialogue;
        }
        public ReoderQuestionResponse parseToReorder(SpeakingQuestion q) {
            var random = new Random();

            var words = q.sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var shuffled = words.OrderBy(_ => random.Next()).ToList();

            ReoderQuestionResponse r = new ReoderQuestionResponse();
            r.sentence = q.sentence;
            r.shuffledWords = shuffled;

            return r;
        }
    }
}
