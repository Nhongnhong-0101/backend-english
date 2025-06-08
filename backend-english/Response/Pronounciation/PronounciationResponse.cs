using Newtonsoft.Json;

namespace backend_english.Response.Pronounciation
{
    public class PronounciationResponse
    {
        public string Id { get; set; }
        public string RecognitionStatus { get; set; }
        public List<NBest> NBest { get; set; }
    }
    public class NBest
    {
        public string Display { get; set; }
        public PronunciationAssessment PronunciationAssessment { get; set; }
        public List<Word> Words { get; set; }
    }

    public class PronunciationAssessment
    {
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double ProsodyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronScore { get; set; }
    }

    public class Word
    {
        public string WordText { get; set; }

        [JsonProperty("PronunciationAssessment")]
        public WordAssessment PronunciationAssessment { get; set; }

        [JsonProperty("Word")]
        public string JsonWord
        {
            set => WordText = value;
        }
    }

    public class WordAssessment
    {
        public double AccuracyScore { get; set; }
        public string ErrorType { get; set; }
    }

}
