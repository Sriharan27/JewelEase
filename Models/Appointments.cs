using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JewelEase.Models
{
	public class Appointments
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int AppointmentId { get; set; }

		[Required]
		public int UserId { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime AppointmentDate { get; set; }

		[Required]
		[DataType(DataType.Time)]
		public TimeSpan AppointmentTime { get; set; }

		public int? ConsultantId { get; set; } = 0;

		public string? Message { get; set; }

        public string? InvitationBody { get; set; }

        [Required]
		[StringLength(50)]
		public string Status { get; set; }

		[ForeignKey("UserId")]
		public User? User { get; set; }
	}
}
