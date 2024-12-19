using Microsoft.AspNetCore.Mvc;
using JewelEase.Models;
using System.Linq;
using System.Threading.Tasks;
using JewelEase.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JewelEase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SaleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet("sales")]
        public async Task<ActionResult<IEnumerable<Sales>>> GetSales()
        {
            return await _context.Sales.Include(s => s.User).ToListAsync();
        }

        // GET: api/Sales/5
        [HttpGet("sales/{id}")]
        public async Task<ActionResult<Sales>> GetSale(int id)
        {
            var sale = await _context.Sales.Include(s => s.User).FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        // POST: api/Sales
        [HttpPost("sales")]
        public async Task<ActionResult<Sales>> PostSale(Sales sale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.SaleId }, sale);
        }

        // PUT: api/Sales/5
        [HttpPut("sales/{id}")]
        public async Task<IActionResult> PutSale(int id, Sales sale)
        {
            if (id != sale.SaleId)
            {
                return BadRequest();
            }

            _context.Entry(sale).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleExists(id))
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

        // DELETE: api/Sales/5
        [HttpDelete("sales/{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.SaleId == id);
        }

        // SalesLineItem CRUD APIs:

        // GET: api/SalesLineItems
        [HttpGet("saleslineitems")]
        public async Task<ActionResult<IEnumerable<SalesLineItem>>> GetSalesLineItems()
        {
            return await _context.SalesLineItem.Include(s => s.JewelryItems).ToListAsync();
        }

        // GET: api/SalesLineItems/5
        [HttpGet("saleslineitems/{saleId}")]
        public async Task<ActionResult<IEnumerable<SalesLineItem>>> GetSalesLineItems(int saleId)
        {
            var lineItems = await _context.SalesLineItem
                                          .Include(s => s.JewelryItems)
                                          .Where(s => s.SaleId == saleId)
                                          .ToListAsync();

            if (!lineItems.Any())
            {
                return NotFound();
            }

            return Ok(lineItems); // Return a list of SalesLineItem objects
        }

        // POST: api/SalesLineItems
        [HttpPost("saleslineitems")]
        public async Task<ActionResult<SalesLineItem>> PostSalesLineItem(SalesLineItem salesLineItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SalesLineItem.Add(salesLineItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalesLineItems), new { id = salesLineItem.SaleLineItemId }, salesLineItem);
        }

        // PUT: api/SalesLineItems/5
        [HttpPut("saleslineitems/{id}")]
        public async Task<IActionResult> PutSalesLineItem(int id, SalesLineItem salesLineItem)
        {
            if (id != salesLineItem.SaleLineItemId)
            {
                return BadRequest();
            }

            _context.Entry(salesLineItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesLineItemExists(id))
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

        // DELETE: api/SalesLineItems/5
        [HttpDelete("saleslineitems/{id}")]
        public async Task<IActionResult> DeleteSalesLineItem(int id)
        {
            var lineItem = await _context.SalesLineItem.FindAsync(id);
            if (lineItem == null)
            {
                return NotFound();
            }

            _context.SalesLineItem.Remove(lineItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SalesLineItemExists(int id)
        {
            return _context.SalesLineItem.Any(e => e.SaleLineItemId == id);
        }
        // Get summary of sales data
        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummary()
        {
            var salesSummary = await _context.Sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    TotalSales = group.Sum(s => s.NetTotal),
                    TotalDiscounts = group.Sum(s => s.Discount),
                    NumberOfSales = group.Count()
                })
                .ToListAsync();

            return Ok(salesSummary);
        }

        // Get details of sales line items
        [HttpGet("sales-lineitems")]
        public async Task<IActionResult> CGetSalesLineItems()
        {
            var salesLineItems = await _context.SalesLineItem
                .Include(sli => sli.JewelryItems)
                .GroupBy(sli => sli.JewelryItemId)
                .Select(group => new
                {
                    JewelryItemId = group.Key,
                    TotalQtySold = group.Sum(sli => sli.Qty),
                    TotalRevenue = group.Sum(sli => sli.TotalPrice)
                })
                .ToListAsync();

            return Ok(salesLineItems);
        }
        [HttpGet("GetSalesData")]
        public async Task<IActionResult> GetSalesData()
        {
            // Fetch sales line items, grouped by month and category
            var salesData = await _context.SalesLineItem
                .Include(sli => sli.Sales) // Access the SaleId in the SalesLineItem table
                .Include(sli => sli.JewelryItems)
                .ThenInclude(ji => ji.Category) // Include category information from JewelryItems
                .GroupBy(sli => new { sli.Sales.SaleDate.Year, sli.Sales.SaleDate.Month }) // Group by Year and Month of the SaleDate
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sales = g.GroupBy(sli => sli.JewelryItems.CategoryId) // Group by CategoryId
                             .Select(cg => new
                             {
                                 CategoryId = cg.Key,
                                 CategoryName = cg.FirstOrDefault().JewelryItems.Category.CategoryName, // Fetch category name from JewelryItems
                                 TotalQty = cg.Sum(sli => sli.Qty) // Sum up the quantity sold per category
                             })
                             .ToList()
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToListAsync();

            return Ok(salesData);
        }
        [HttpGet("getMonthlySales")]
        public async Task<IActionResult> GetMonthlySales()
        {
            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var salesData = await _context.Sales
                .Where(s => s.SaleDate >= firstDayOfMonth && s.SaleDate <= lastDayOfMonth)
                .GroupBy(s => s.SaleDate.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalSales = g.Sum(s => s.NetTotal),
                    Date = g.Key
                })
                .OrderBy(s => s.Day)
                .ToListAsync();

            var totalSalesAmount = salesData.Sum(s => s.TotalSales);

            // Create a list of day names (e.g., "March 1", "March 2", etc.)
            var dayLabels = salesData.Select(s => $"{currentDate:MMMM} {s.Day}").ToList();

            var response = new
            {
                TotalSales = totalSalesAmount,
                DailySales = salesData.Select(s => s.TotalSales).ToList(),
                Labels = dayLabels
            };

            return Ok(response);
        }
        [HttpGet]
        [Route("totalItemsSoldThisMonth")]
        public IActionResult GetTotalItemsSoldThisMonth()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var totalItemsSold = _context.SalesLineItem
                .Where(s => s.Sales.SaleDate.Year == currentYear && s.Sales.SaleDate.Month == currentMonth)
                .Sum(s => s.Qty);

            return Ok(new { totalItemsSold });
        }
        [HttpGet("GetSalesDataByItem")]
        public async Task<IActionResult> GetSalesDataByItem()
        {
            // Fetch sales line items, grouped by month and jewelry item
            var salesData = await _context.SalesLineItem
                .Include(sli => sli.Sales) // Access the SaleId in the SalesLineItem table
                .Include(sli => sli.JewelryItems)
                    .ThenInclude(ji => ji.Category) // Include category information from JewelryItems
                .GroupBy(sli => new { sli.Sales.SaleDate.Year, sli.Sales.SaleDate.Month, sli.JewelryItems.JewelryItemId }) // Group by Year, Month, and Jewelry Item
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    ItemId = g.Key.JewelryItemId,
                    ItemName = g.FirstOrDefault().JewelryItems.Name, // Fetch item name
                    CategoryId = g.FirstOrDefault().JewelryItems.CategoryId, // Fetch category ID
                    CategoryName = g.FirstOrDefault().JewelryItems.Category.CategoryName, // Fetch category name
                    TotalQty = g.Sum(sli => sli.Qty) // Sum up the quantity sold per item
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToListAsync();

            return Ok(salesData);
        }

    }
}
