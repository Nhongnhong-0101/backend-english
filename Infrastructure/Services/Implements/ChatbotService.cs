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
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        
        private string gptKey  = String.Empty;
        private string filePath = String.Empty;
        private string direction = "You are an English tutor";
        private static readonly HttpClient client = new HttpClient();


        public ChatbotService(IConfiguration configuration)
        {
             gptKey = configuration["GPT:Chatbot_key"];
        }

        public async Task<string> SendChatMessageAsync(string message)
        {

            ChatClient client = new(
                model: "gpt-3.5-turbo",
                apiKey: gptKey
            );

            List<ChatMessage> messages =
            [
                new SystemChatMessage(direction),
                new UserChatMessage("Hello!")
            ];

            ChatCompletion completion = client.CompleteChat(messages);

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

        public async Task<ChatResponse> ProcessSpeechAsync(IFormFile recored)
        {
            try
            {
                ChatResponse chat = new ChatResponse();

                var text = await TranscriptAudioAsync(recored); 

                var response = await SendChatMessageAsync(text);

                
                chat.transcribedText = text;
                chat.responseFromAI = response;

                return chat;
            }
            catch (Exception ex)
            {
                return new ChatResponse();
            }
        }
    }
}
