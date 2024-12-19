using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelEase.Models
{
    public class Sales
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleId { get; set; }

        [Required]
        public string? SaleRefId { get; set; }

        [Required]
        public int UserId { get; set; }

		[Required]
		public int Discount { get; set; }

		[Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetTotal{ get; set; }

		[Required]
		public DateTime SaleDate { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
