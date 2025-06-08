namespace backend_english.Response.Pronounciation
{
    public class AssessmentResponse
    {
        public string DisplayText { get; set; }
        public double OverallAccuracy { get; set; }
        public double Fluency { get; set; }
        public double Prosody { get; set; }
        public double Completeness { get; set; }
        public double PronScore { get; set; }
        public List<WordResult> Words { get; set; }
    }

    public class WordResult
    {
        public string Word { get; set; }
        public double Score { get; set; }
        public string ErrorType { get; set; }
    }
}
