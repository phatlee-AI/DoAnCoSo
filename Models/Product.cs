using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyWebApp.Models;

namespace MyWebApp.Models
{
    [Table("Product")]
    public class Product
    {
        public Product()
        {
            this.ProductImages = new HashSet<ProductImage>();
        }
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 1000000000009.00)]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}
