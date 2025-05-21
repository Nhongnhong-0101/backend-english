namespace backend_english.Request
{
    public class PronunciationRequest
    {
        public IFormFile audio {  get; set; }
        public string sentence { get; set; }
    }
}
