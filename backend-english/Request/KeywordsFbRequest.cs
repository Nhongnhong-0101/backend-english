namespace backend_english.Request
{
    public class KeywordsFbRequest
    {
        public List<string> keywords { get; set; } = new();
        public int level { get; set; }
        public IFormFile audio { get; set; }
    }
}
