namespace backend_english.Request
{
    public class CompleteLessonRequest
    {
        public Guid accountId { get; set; }
        public Guid lessonId { get; set; }
    }
}
