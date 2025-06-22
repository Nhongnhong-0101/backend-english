namespace backend_english.Request
{
    public class KeywordsFbRequest
    {
        public List<string> keywords { get; set; } = new();
        public string userSentence { get; set; } = string.Empty;
    }
}
