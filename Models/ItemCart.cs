using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApp.Models
{
    [Table("ItemCart")]
    public class ItemCart
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }

        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }
    }
}
