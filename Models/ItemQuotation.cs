using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelEase.Models
{
    public class ItemQuotation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuotationId { get; set; }

        [Required]
        public int UserId { get; set; }

		[Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; }

		[Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal QuotationPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

		[Required]
		public DateTime RequestedDate { get; set; }

        public DateTime? SentDate { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
