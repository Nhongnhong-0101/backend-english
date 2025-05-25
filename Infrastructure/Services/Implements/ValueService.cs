using backend_english.Response.Pronounciation;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class ValueService : IValueService
    {
        private string apiKey = string.Empty;
        private string region = string.Empty;
        private string filePath = string.Empty;
        public ValueService(IConfiguration configuration)
        {
            apiKey = configuration["Speech_api:Access_pronounciation"];
            region = configuration["Speech_api:Region"];

        }
        public AssessmentResponse FakeResponse()
        {
            var ac = new AssessmentResponse();
            ac.DisplayText = "Repeat this sentence aloud";
            ac.OverallAccuracy = 80.0f;
            ac.Fluency = 70.0f;
            ac.Prosody = 75.0f;
            ac.Completeness = 90.0f;
            ac.PronScore = 78.0f;
            ac.Words = new List<WordResult>();
            var a = new WordResult { Word = "Repeat", Score = 85.0f, ErrorType = "None" };
            var b = new WordResult { Word = "this", Score = 92.0f, ErrorType = "None" };
            var c = new WordResult { Word = "sentence", Score = 65.0f, ErrorType = "Mispronunciation" };
            var d = new WordResult { Word = "aloud", Score = 50.0f, ErrorType = "Mispronunciation" };
            ac.Words.Add(a);
            ac.Words.Add(b);
            ac.Words.Add(c);
            ac.Words.Add(d);
            return ac;


        }

        public async Task<AssessmentResponse?> sendToAzure(string sentence, IFormFile audio)
        {
            try
            {
                filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await audio.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(region))
                {
                    SpeechConfig speechConfig = SpeechConfig.FromSubscription(apiKey, region);
                    speechConfig.SpeechRecognitionLanguage = "en-US";
                        var pronunciationConfig = new PronunciationAssessmentConfig(
                        sentence,
                      GradingSystem.HundredMark,
                      Granularity.Word,
                      true);
                    pronunciationConfig.EnableProsodyAssessment();
                    using (var audioConfig = AudioConfig.FromWavFileInput(filePath))
                    using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                    {
                        pronunciationConfig.ApplyTo(recognizer);
                        var speechRecognitionResult = await recognizer.RecognizeOnceAsync();
                        var pronunciationAssessmentResult = PronunciationAssessmentResult.FromResult(speechRecognitionResult);
                        var pronunciationAssessmentResultJson = speechRecognitionResult.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

                        var result = JsonConvert.DeserializeObject<PronounciationResponse>(pronunciationAssessmentResultJson);
                        var nBest = result.NBest.First();
                        var assessment = nBest.PronunciationAssessment;

                        var wordResults = new List<WordResult>();
                        foreach (var w in nBest.Words)
                        {
                            var wr = new WordResult();
                            wr.Word = w.WordText;
                            wr.Score = w.PronunciationAssessment.AccuracyScore;
                            wr.ErrorType = w.PronunciationAssessment.ErrorType;
                            wordResults.Add(wr);
                        }

                        var response = new AssessmentResponse();
                        response.DisplayText = sentence;
                        response.OverallAccuracy = assessment.AccuracyScore;
                        response.Fluency = assessment.FluencyScore;
                        response.Prosody = assessment.ProsodyScore;
                        response.Completeness = assessment.CompletenessScore;
                        response.PronScore = assessment.PronScore;
                        response.Words = wordResults;
                        ;

                        return response;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"sendToAzure error: {ex.Message}");
                return null;
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                filePath = String.Empty;
            }
        }
    }
}
