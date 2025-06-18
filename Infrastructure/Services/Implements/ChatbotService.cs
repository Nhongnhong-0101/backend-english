using Infrastructure.Services.Interfaces;
using Infrastructure.Services.Response;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        
        private string gptKey  = String.Empty;
        private string filePath = String.Empty;
        private string direction = "You are an English tutor, help me learn to communicate effectively.";
        private string mainTopic = String.Empty;
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

            ChatClient client = new(
                model: "gpt-4o-mini",
                apiKey: gptKey
            );
           
            ChatCompletion completion = client.CompleteChat(chatHistory);

            Console.WriteLine(completion.Content[0].Text);
            return completion.Content[0].Text;  
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
        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("\\n", " ").Replace("\"", "").Trim();
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
    }
}
