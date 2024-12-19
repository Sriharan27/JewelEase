using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelEase.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using JewelEase.Data;

namespace JewelEase.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentApiController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public AppointmentApiController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/appointments
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Appointments>>> GetAppointments()
		{
			return await _context.Appointments.Include(a => a.User).ToListAsync();
		}

		// GET: api/appointments/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<Appointments>> GetAppointment(int id)
		{
			var appointment = await _context.Appointments.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.AppointmentId == id);

			if (appointment == null)
			{
				return NotFound();
			}

			return appointment;
		}

		// POST: api/appointments
		[HttpPost]
		public async Task<ActionResult<Appointments>> CreateAppointment(Appointments appointment)
		{
			appointment.AppointmentDate = DateTime.Parse(appointment.AppointmentDate.ToString());
			appointment.AppointmentTime = TimeSpan.Parse(appointment.AppointmentTime.ToString());

			_context.Appointments.Add(appointment);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
		}

		// PUT: api/appointments/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointment(int id, Appointments appointment)
		{
			if (id != appointment.AppointmentId)
			{
				return BadRequest();
			}

			_context.Entry(appointment).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AppointmentExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		// DELETE: api/appointments/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAppointment(int id)
		{
			var appointment = await _context.Appointments.FindAsync(id);
			if (appointment == null)
			{
				return NotFound();
			}

			_context.Appointments.Remove(appointment);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool AppointmentExists(int id)
		{
			return _context.Appointments.Any(e => e.AppointmentId == id);
		}
	}
}
