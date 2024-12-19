using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
	public class SalesLineItem
    {
		[Key]
		public int SaleLineItemId { get; set; }

		[Required]
		public int SaleId { get; set; }

		[Required]
		public int JewelryItemId { get; set; }

		[Required]
		public int Qty { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalPrice { get; set; }

		[ForeignKey("SaleId")]
		public Sales? Sales { get; set; }

        [ForeignKey("JewelryItemId")]
        public JewelryItems? JewelryItems { get; set; }
    }
}
