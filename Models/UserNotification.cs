namespace MyWebApp.Models
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }          // ID người dùng nhận thông báo
        public string Message { get; set; }         // Nội dung phản hồi từ admin
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
