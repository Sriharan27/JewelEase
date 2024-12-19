using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
	public class ItemQuotationLineItem
	{
		[Key]
		public int QuotationLineItemId { get; set; }

		[Required]
		public int QuotationId { get; set; }

		[Required]
		public int JewelryItemId { get; set; }

		[Required]
		public int Qty { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalPrice { get; set; }

		[ForeignKey("QuotationId")]
		public ItemQuotation? ItemQuotation { get; set; }

        [ForeignKey("JewelryItemId")]
        public JewelryItems? JewelryItems { get; set; }
    }
}
