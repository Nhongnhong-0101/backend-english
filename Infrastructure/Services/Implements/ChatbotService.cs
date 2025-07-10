using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech.Diagnostics.Logging;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Infrastructure.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        
        private string gptKey  = String.Empty;
        private string filePath = String.Empty;
        private string direction = "You are an English tutor, help me learn to communicate effectively.";
        private string mainTopic = String.Empty;
        private List<ChatMessageRecord> conversationHistory = new();
        private static readonly HttpClient client = new HttpClient();
        private readonly ISQuestionService questionService;

        private List<ChatMessage> chatMessages = new List<ChatMessage>();
        private List<Guid> usedQuestion = new List<Guid>();


        public ChatbotService(IConfiguration configuration, ISQuestionService sQuestionService)
        {
             gptKey = configuration["GPT:Chatbot_key"];
            questionService = sQuestionService;
        }

        public async Task<string> SendToGPTAsync(List<ChatMessage> chatHistory)
        {
            try
            {
                ChatClient client = new(
                    model: "gpt-4o-mini",
                    apiKey: gptKey
                );

                ChatCompletion completion = client.CompleteChat(chatHistory);

                Console.WriteLine(completion.Content[0].Text);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error calling GPT: " + ex.Message);
                return null;
            }
        }

        public async Task<string> TranscriptAudioAsync(IFormFile recored)
        {
            var fileExtension = Path.GetExtension(recored.FileName).ToLower();
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{fileExtension}");
            try
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    await recored.CopyToAsync(stream);
                    stream.Flush();
                }

                AudioClient client = new(
                    model: "whisper-1",
                    apiKey: gptKey
                );

                AudioTranscription transcription = client.TranscribeAudio(filePath);

                Console.WriteLine($"{transcription.Text}");

                return transcription.Text;

            }

            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                filePath = String.Empty;
            }
        }

        public async Task<String> StartConversationAsync(string topic)
        {
            try
            {
                mainTopic = topic.ToLower();
                var question = await questionService.GetFirstQuestionInTopic(mainTopic);
                SystemChatMessage systemChatMessage;
                if (question != null)
                {
                    systemChatMessage = new SystemChatMessage($"""
                        You are an English teacher helping a student practice conversation on the topic: {mainTopic}.
                    Please respond in natural, friendly, and encouraging paragraphs.
                    Avoid using numbered lists, bullet points, or fragmented sentences.
                    Politely correct any grammar or vocabulary errors within your response, helping the student learn without discouraging them.
                    Use clear and simple language suitable for intermediate English learners.
                    Keep the conversation focused on the topic, and provide questions or comments to guide the student.
                    And please use this question {question.sentence} to guide the student.
                    """);
                }
                else
                {
                    systemChatMessage = new SystemChatMessage($"""
                        You are an English teacher helping a student practice conversation on the topic: {mainTopic}.
                    Please respond in natural, friendly, and encouraging paragraphs.
                    Avoid using numbered lists, bullet points, or fragmented sentences.
                    Politely correct any grammar or vocabulary errors within your response, helping the student learn without discouraging them.
                    Use clear and simple language suitable for intermediate English learners.
                    Keep the conversation focused on the topic, and provide questions or comments to guide the student.
                    """);
                }

                    var messages = new List<ChatMessage>
                {
                    systemChatMessage,
                     new UserChatMessage($"Let's start a conversation about {topic}. Give me a question to begin."),
                };
                var firstQuestion = await SendToGPTAsync(messages);
                firstQuestion = CleanText(firstQuestion);
                messages.Add(new AssistantChatMessage(firstQuestion));

                chatMessages.AddRange(messages);

                return firstQuestion;
            }
            catch
            {
                throw;
            }
        }

        public async Task<String> ContinuteConversationAsync(string userInput)
        {
            try
            {
                var embedingInout = await questionService.GetEmbeddingAsync(userInput);
                var nextQuestion = await questionService.GetNextQuestionByEmbeddingAsync(embedingInout, mainTopic, usedQuestion);
                SystemChatMessage systemChatMessage;
                if (nextQuestion != null)
                {
                    systemChatMessage = new SystemChatMessage($"Keep focus on {mainTopic} and use this question if it is reasonable {nextQuestion.sentence}");
                }
                else
                {
                    systemChatMessage = new SystemChatMessage($"Keep focus on {mainTopic}");
                }
                    var messages = new List<ChatMessage>
                {
                    systemChatMessage,
                     new UserChatMessage(userInput),
                };
                UserChatMessage userMessage = new UserChatMessage(userInput);
                chatMessages.Add(userMessage);
                var reply = await SendToGPTAsync(chatMessages);
                reply = CleanText(reply);
                chatMessages.Add(new AssistantChatMessage(reply));
                return reply;
            }
            catch
            {
                throw;
            }
        }

        public void EndConversation()
        {
            chatMessages.Clear();
            mainTopic = String.Empty;
        }

        public async Task<String> ReplyUserAudio(IFormFile recored)
        {
            try
            {
                var userInput = await TranscriptAudioAsync(recored);
                
                return await ContinuteConversationAsync (userInput);

            }
            catch
            {
                throw;
            }
        }

        public async Task<List<KeywordsResponse>> GetKeywordsFromSentenceAsync(List<string> sentence)
        {
            try
            {
                string prompt = $@"
                You are a helpful assistant that extracts important keywords from English sentences.
                Given a list of sentences, return a JSON array. Each item must contain the original sentence and its extracted keywords (nouns, verbs, or important phrases). 
                Output format:

                [
                  {{ ""sentence"": ""..."", ""keywords"": [""..."", ""...""] }},
                  ...
                ]

                Here are the sentences:

                {string.Join("\n", sentence.Select((s, i) => $"{i + 1}. {s}"))}

                Only return valid JSON. Do not include any explanation or markdown.
                ";

                SystemChatMessage systemChatMessage;
                systemChatMessage = new SystemChatMessage(prompt);
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are an English teacher assistant."),
                    new UserChatMessage(prompt),
                };
                var response = await SendToGPTAsync(messages);
                response = CleanJson(response);
                var result = new List<KeywordsResponse>();

                using var doc = JsonDocument.Parse(response);
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var sen = element.GetProperty("sentence").GetString();
                    var keywords = element.GetProperty("keywords").EnumerateArray().Select(k => k.GetString()).ToList();

                    KeywordsResponse key = new KeywordsResponse();
                    key.keywords = keywords;
                    key.sentence = sen;
                    result.Add(key);
                }

                return result;

            }
            catch
            {
                throw;
            }
        }
        public async Task<KeywordsFbResponse> GetKeywordsFeedbackAsync(List<string> keywords, string userSentence, int level)
        {
            try
            {
                string prompt = "";
                string keywordsJson = JsonSerializer.Serialize(keywords);

                switch (level)
                {
                    case 1:
                        prompt = $@"
                                Bạn là một giáo viên dạy tiếng Anh cho người mới bắt đầu.

                                Học viên được giao các từ khóa: {keywordsJson}
                                Học viên đã viết câu: ""{userSentence}""

                                Nhiệm vụ của bạn:
                                1. Kiểm tra câu học viên viết có đúng ngữ pháp không, trả lời ngắn gọn.
                                2. Nếu có lỗi ngữ pháp, hãy sửa lại câu đó.
                                3. Giải thích lỗi bằng tiếng Việt dễ hiểu.

                                ⚠️ Chỉ trả về đúng **định dạng JSON object** sau, **không thêm chú thích, không bao gồm dấu ```json**:
                                    {{
                                      ""evaluation"": ""(Câu này đúng hay sai ngữ pháp)"",
                                      ""suggestion"": ""(Nếu có sửa thì ghi ở đây, còn không thì giữ nguyên)"",
                                      ""explanation"": ""(Giải thích bằng tiếng Việt)""
                                    }}";
                        break;

                    case 2:
                        prompt = $@"
                                Bạn là một giáo viên dạy tiếng Anh ở mức độ B1.

                                Học viên được giao các từ khóa: {keywordsJson}
                                Học viên đã viết câu: ""{userSentence}""

                                Nhiệm vụ của bạn:
                                1. Đánh giá câu học viên viết (đúng/sai ngữ pháp, tự nhiên hay không).
                                2. Gợi ý cách viết tốt hơn (sử dụng từ khóa nếu có thể).
                                3. Giải thích vì sao cách viết đó tốt hơn.

                                ⚠️ Chỉ trả về đúng **định dạng JSON object** sau, **không thêm chú thích, không bao gồm dấu ```json**:
                                {{
                                  ""evaluation"": ""(Nhận xét chung về câu của học viên bằng tiếng Việt)"",
                                  ""suggestion"": ""(Gợi ý câu hay hơn)"",
                                  ""explanation"": ""(Giải thích lý do và cách dùng từ/cấu trúc)""
                                }}";
                        break;

                    case 3:
                    default:
                        prompt = $@"
                                You are an advanced English teacher for level A1.

                                The student was given these keywords: {keywordsJson}
                                The student wrote: ""{userSentence}""

                                Your tasks:
                                1. Evaluate the grammar and naturalness of the sentence.
                                2. Suggest a better version using the keywords (if needed).
                                3. Explain why the suggestion is better.

                                ⚠️ Only return a valid **JSON object**, no extra explanation, no triple backticks:
                                    {{
                                      ""evaluation"": ""(Short assessment)"",
                                      ""suggestion"": ""(Improved sentence)"",
                                      ""explanation"": ""(Why it's better)""
                                    }}";    
                        break;
                }

                SystemChatMessage systemChatMessage;
                systemChatMessage = new SystemChatMessage(prompt);
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful and professional English teacher."),
                    new UserChatMessage(prompt),
                };
                var response = await SendToGPTAsync(messages);
                response = CleanJson(response);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;
                var evaluation = root.GetProperty("evaluation").GetString();
                var suggestion = root.GetProperty("suggestion").GetString();
                var explanation = root.GetProperty("explanation").GetString();

                KeywordsFbResponse fb = new KeywordsFbResponse();
                fb.userSentence = userSentence;
                fb.evaluation = evaluation;
                fb.suggestion = suggestion;
                fb.explanation = explanation;

                return fb;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetKeywordsFeedbackAsync: {ex.Message}");
                return new KeywordsFbResponse
                {
                    evaluation = "Đã xảy ra lỗi trong quá trình xử lý.",
                    suggestion = "",
                    explanation = ex.Message
                };
            }
        }
        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("\\n", " ").Replace("\"", "").Trim();
        }
        private string CleanJson(string content)
        {

            content = content.Trim('`', '\n', ' ');
            if (content.StartsWith("```json"))
            {
                var match = Regex.Match(content, @"```json\s*(\[\s*{.+}\s*\])\s*```", RegexOptions.Singleline);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            return content;
        }

        public async Task<string> FinishChatAndGetReview(String topic, int level, List<ChatMessageRecord> chatHistory)
        {
            try
            {

                string history = string.Join("\n", chatHistory.Select(m => $"{m.role.ToUpper()}: {m.message}"));

                string prompt = level switch
                {
                    1 or 2 => $@"
                            Bạn là một giáo viên tiếng Anh.
                            Chủ đề hội thoại: {topic}
                            Cấp độ của học viên: {level}

                            Dưới đây là toàn bộ đoạn hội thoại giữa ban:

                            {history}

                            Hãy nhận xét tổng quan về cuộc hội thoại của học viên:
                            1. Câu trả lời của bạn có đúng trọng tâm và phù hợp với chủ đề không?
                            2. Cách dùng từ và diễn đạt của bạn có tự nhiên chưa?
                            3. Có lỗi ngữ pháp hay cách diễn đạt nào cần lưu ý không? (Nếu có thì chỉ ra rõ)
                            4. Gợi ý những điểm bạn nên cải thiện hoặc học thêm để nâng cao khả năng giao tiếp tiếng Anh.

                            Trả lời bằng tiếng Việt. Xưng hô với người học là 'bạn'",

                    3 => $@"
                            You are an English teacher.
                            The topic of conversation is {topic}
                            Level of student is {level}

                            Here is the full conversation between the student and the AI assistant:

                            {history}

                            Please give a general assessment of the student's communication skills by answering the following:

                            1. Are the student's answers relevant and focused on the topic?
                            2. Are the student's word choices and expressions natural?
                            3. Are there any grammar, pronunciation, or expression mistakes? (If yes, please point them out)
                            4. Suggest what the student should improve or learn to enhance their English speaking skills.

                            Respond in English.",
                    _ => throw new ArgumentOutOfRangeException(nameof(level))
                };

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are an English teacher assistant."),
                    new UserChatMessage(prompt),
                };


                string review = await SendToGPTAsync(messages);

                if (review != null)
                {
                    ClearSession();
                    return review;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in FinishChatAndGetReview: {ex.Message}");
                return null;
            }
        }
         

        public void AddMessage(string role, string message, int level)
        {
            conversationHistory.Add(new ChatMessageRecord
            {
                role = role,
                message = message,
                timestamp = DateTime.UtcNow,
                level = level
            });
        }

        public List<ChatMessageRecord> GetConversationHistory()
        {
            return conversationHistory;
        }

        public void ClearSession()
        {
            conversationHistory.Clear();
        }

        public async Task<float[]> CreateEmbeddingAsync(string sentence)
        {
            try
            {
                EmbeddingClient client = new(
                    model: "text-embedding-3-small",
                    apiKey: gptKey
                );
                var embeddingResult = await client.GenerateEmbeddingAsync(sentence);

                if (embeddingResult == null || embeddingResult.Value == null)
                {
                    Console.WriteLine("Không tạo được embedding.");
                    return null;
                }

                return embeddingResult.Value.ToFloats().ToArray();

            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error calling GPT: " + ex.Message);
                return null;
            }
        }
    }
}
