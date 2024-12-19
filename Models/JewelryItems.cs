using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
    public class JewelryItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JewelryItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(800)]
        public string Description { get; set; }

        [Required]
        public int karats { get; set; }

        public int Price { get; set; } = 0;

        [Required]
        public int StockLevel { get; set; }

        [Required]
        public string IdentificationID { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(255)]
        public string TryOnUrl { get; set; }

        public string? Status { get; set; } = "Active"; 

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } 
    }
}
