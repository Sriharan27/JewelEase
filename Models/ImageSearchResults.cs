using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
    public class ImageSearchResults
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SearchResultId { get; set; }

        [Required]
        [StringLength(255)]
        public string UploadedImageUrl { get; set; }

        [Required]
        public int MatchingJewelryItemId { get; set; }

        [Required]
        public DateTime SearchDate { get; set; }

        [ForeignKey("MatchingJewelryItemId")]
        public JewelryItems JewelryItem { get; set; }
    }
}
