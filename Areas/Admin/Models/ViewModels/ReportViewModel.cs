namespace MyWebApp.Areas.Admin.Models.ViewModels
{
    public class ReportViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
