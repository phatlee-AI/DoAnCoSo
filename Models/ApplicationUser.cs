using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
