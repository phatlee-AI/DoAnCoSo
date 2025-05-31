namespace MyWebApp.Models
{
    public class ReportFeedback
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public string? AdminReply { get; set; }  // Phản hồi từ admin
        public DateTime? RepliedAt { get; set; }
        public string UserId { get; set; }

    }
}