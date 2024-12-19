using JewelEase.Data;
using JewelEase.Models;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace JewelEase.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class QuotationApiController : Controller
	{
		private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public QuotationApiController(ApplicationDbContext context, EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// Get all ItemQuotations
		[HttpGet("quotations")]
		public IActionResult GetQuotations()
		{
			var quotations = _context.ItemQuotation.Include(q => q.User).ToList();
			return Ok(quotations);
		}

		// Get specific ItemQuotation by id
		[HttpGet("quotations/{id}")]
		public IActionResult GetQuotation(int id)
		{
			var quotation = _context.ItemQuotation.Include(q => q.User).FirstOrDefault(q => q.QuotationId == id);
			if (quotation == null)
				return NotFound();

			return Ok(quotation);
		}

		// Create a new ItemQuotation
		[HttpPost("CreateItemQuotation")]
		public async Task<IActionResult> CreateItemQuotation(ItemQuotation itemQuotation)
        {
            if (ModelState.IsValid)
			{
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == itemQuotation.UserId);

                itemQuotation.RequestedDate = DateTime.Now;
				_context.ItemQuotation.Add(itemQuotation);
				await _context.SaveChangesAsync();

                var message = $"JewelEase\n\n" +
                              $"Hi {user.Name},\n\n" +
                              "Thank you for reaching out to us! We have received your quotation request.\n\n" +
                              $"Your Quotation ID: {itemQuotation.QuotationId}\n\n" +
                              "Our team is working on your request, and you will receive a detailed quotation shortly.\n\n" +
                              "Best regards,\nThe JewelEase Team";

                var emailBody = message;

                // Send the quotation confirmation email
                await _emailService.SendEmailAsync(user.Email, "Quotation Request Received", emailBody);

                return Ok(new { success = true, quotationId = itemQuotation.QuotationId });
            }
            return BadRequest(new { success = false });
        }

        // Update an existing ItemQuotation
		[HttpPut("quotations/{id}")]
		public async Task<IActionResult> UpdateQuotation(int id, [FromBody] ItemQuotation quotation)
		{
			if (id != quotation.QuotationId || !ModelState.IsValid)
			{
				return BadRequest();
			}

			_context.Entry(quotation).State = EntityState.Modified;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.ItemQuotation.Any(q => q.QuotationId == id))
				{
					return NotFound();
				}
				throw;
			}

			return Ok(new { success = true });
		}

		// Delete an ItemQuotation
		[HttpDelete("quotations/{id}")]
		public async Task<IActionResult> DeleteQuotation(int id)
		{
			var quotation = await _context.ItemQuotation.FindAsync(id);
			if (quotation == null)
			{
				return NotFound();
			}

			_context.ItemQuotation.Remove(quotation);
			await _context.SaveChangesAsync();

			return Ok(new { success = true });
		}

		// Get all ItemQuotationLineItems by QuotationId
		[HttpGet("quotations/{quotationId}/lineitems")]
		public IActionResult GetQuotationLineItems(int quotationId)
		{
			var lineItems = _context.ItemQuotationLineItem
				.Where(li => li.QuotationId == quotationId)
				.Include(li => li.ItemQuotation)
				.ToList();
			return Ok(lineItems);
		}

		// Create a new ItemQuotationLineItem
		[HttpPost("CreateLineItems")]
		public async Task<IActionResult> CreateLineItems(List<ItemQuotationLineItem> lineItems)
		{
			if (ModelState.IsValid)
			{
				_context.ItemQuotationLineItem.AddRange(lineItems);
				await _context.SaveChangesAsync();
				return Ok(new { success = true });
			}
			return BadRequest(new { success = false });
		}

		// Update an ItemQuotationLineItem
		[HttpPut("lineitems/{id}")]
		public async Task<IActionResult> UpdateLineItem(int id, [FromBody] ItemQuotationLineItem lineItem)
		{
			if (id != lineItem.QuotationLineItemId || !ModelState.IsValid)
			{
				return BadRequest();
			}

			_context.Entry(lineItem).State = EntityState.Modified;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.ItemQuotationLineItem.Any(li => li.QuotationLineItemId == id))
				{
					return NotFound();
				}
				throw;
			}

			return Ok(new { success = true });
		}

		// Delete an ItemQuotationLineItem
		[HttpDelete("lineitems/{id}")]
		public async Task<IActionResult> DeleteLineItem(int id)
		{
			var lineItem = await _context.ItemQuotationLineItem.FindAsync(id);
			if (lineItem == null)
			{
				return NotFound();
			}

			_context.ItemQuotationLineItem.Remove(lineItem);
			await _context.SaveChangesAsync();

			return Ok(new { success = true });
		}
	}
}
