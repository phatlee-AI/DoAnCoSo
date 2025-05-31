// Models/Article.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApp.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [StringLength(500)]
        public string Summary { get; set; }

        [StringLength(100)]
        public string ImageUrl { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public bool IsPublished { get; set; } = false;

        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public ApplicationUser Author { get; set; }

        [StringLength(100)]
        public string Slug { get; set; }

        [StringLength(100)]
        public string MetaTitle { get; set; }

        [StringLength(300)]
        public string MetaDescription { get; set; }

        [StringLength(100)]
        public string Tags { get; set; }
    }
}