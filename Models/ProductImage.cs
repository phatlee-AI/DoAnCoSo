using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyWebApp.Models;

namespace MyWebApp.Models
{
    [Table("ProductImage")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
