using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
    public class Slider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SliderId { get; set; }
        public string? MainHeading { get; set; }
        public string? TextDescription { get; set; }
        [Required]
        public string RedirectPage { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public int Order { get; set; }

    }
}
