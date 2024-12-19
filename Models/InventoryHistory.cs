using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
    public class InventoryHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryHistoryId { get; set; }

        [Required]
        public int JewelryItemId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int ChangeAmount { get; set; }

        [Required]
        public int NewStockLevel { get; set; }

        [StringLength(255)]
        public string Reason { get; set; }

        [ForeignKey("JewelryItemId")]
        public JewelryItems JewelryItem { get; set; }
    }
}
