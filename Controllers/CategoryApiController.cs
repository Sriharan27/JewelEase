using Microsoft.AspNetCore.Mvc;
using JewelEase.Models;
using System.Linq;
using System.Threading.Tasks;
using JewelEase.Data;

namespace JewelEase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _context.Category.ToList();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Category.Add(category);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Category.Update(category);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false });
        }
    }
}
