using JewelEase.Data;
using JewelEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XObjdetect;

[Route("api/[controller]")]
[ApiController]
public class JewelryItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JewelryItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/jewelryitemsapi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JewelryItems>>> GetJewelryItems()
    {
        var jewelryItems = await _context.JewelryItems
            .Include(j => j.Category) // Include Category for category name
            .ToListAsync();

        return Ok(jewelryItems.Select(j => new
        {
            j.JewelryItemId,
            j.Name,
            CategoryName = j.Category.CategoryName, // Assuming you have a CategoryName property in Category
            j.Description,
            j.karats,
            j.Price,
            j.StockLevel,
            j.ImageUrl
        }));
    }

    // POST: api/jewelryitemsapi
    [HttpPost]
    public async Task<ActionResult> PostJewelryItem(JewelryItems jewelryItem)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        jewelryItem.Category = null;

        _context.JewelryItems.Add(jewelryItem);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // PUT: api/jewelryitemsapi
    [HttpPut]
    public async Task<ActionResult> PutJewelryItem(JewelryItems jewelryItem)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Entry(jewelryItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!JewelryItemExists(jewelryItem.JewelryItemId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(new { success = true });
    }

    // DELETE: api/jewelryitemsapi/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJewelryItem(int id)
    {
        var jewelryItem = await _context.JewelryItems.FindAsync(id);
        if (jewelryItem == null)
        {
            return NotFound();
        }

        _context.JewelryItems.Remove(jewelryItem);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private bool JewelryItemExists(int id)
    {
        return _context.JewelryItems.Any(e => e.JewelryItemId == id);
    }

    //api/jewelryitemsapi/lowstock
    [HttpGet("lowstock")]
    public IActionResult GetLowStockJewelryItems()
    {
        var lowStockItems = _context.JewelryItems
            .Where(item => item.StockLevel < 10)
            .Include(item => item.Category) // To fetch related category data
            .Select(item => new
            {
                item.JewelryItemId,
                item.Name,
                item.IdentificationID,
                item.StockLevel,
                CategoryName = item.Category.CategoryName // Assuming 'Category' has a 'Name' field
            })
            .ToList();

        return Ok(lowStockItems);
    }
	//api/jewelryitemsapi/searchimage
	[HttpPost("searchimage")]
	public async Task<IActionResult> SearchByImage(IFormFile image)
	{
		if (image == null || image.Length == 0)
		{
			return BadRequest("No image uploaded.");
		}

		var tempPath = Path.Combine(Path.GetTempPath(), image.FileName);
		using (var stream = new FileStream(tempPath, FileMode.Create))
		{
			await image.CopyToAsync(stream);
		}

		List<string> matchingImagePaths = new List<string>();
		double matchThreshold = 0.0125; 

		var categories = await _context.Category.Select(c => c.CategoryName).ToListAsync();

		foreach (var category in categories)
		{
			var categoryFolder = Path.Combine("wwwroot/images", category);
			var imagesInCategory = Directory.GetFiles(categoryFolder);

			foreach (var imgPath in imagesInCategory)
			{
				double matchScore = CompareImages(tempPath, imgPath); 

				if (matchScore > matchThreshold)
				{
					matchingImagePaths.Add(imgPath); 

				}
			}
		}

		if (matchingImagePaths.Count > 0)
		{
			var formattedPaths = matchingImagePaths
				.Select(path => path.Replace("wwwroot\\", "").Replace("wwwroot/", "").Replace("\\", "/"))
				.ToList();

			var matchingItems = await _context.JewelryItems
				.Where(item => formattedPaths.Contains(item.ImageUrl.Replace("\\", "/")))
				.ToListAsync();

			return Ok(matchingItems);
		}

		return NotFound(new { message = "No matching items found." });
	}

	private double CompareImages(string imagePath1, string imagePath2)
	{
		using (Mat img1 = CvInvoke.Imread(imagePath1, ImreadModes.Color))
		using (Mat img2 = CvInvoke.Imread(imagePath2, ImreadModes.Color))
		{
			if (img1.IsEmpty || img2.IsEmpty)
			{
				return 0; 
			}

			Mat gray1 = new Mat();
			Mat gray2 = new Mat();
			CvInvoke.CvtColor(img1, gray1, ColorConversion.Bgr2Gray);
			CvInvoke.CvtColor(img2, gray2, ColorConversion.Bgr2Gray);

			var akaze = new AKAZE(); 
			VectorOfKeyPoint keyPoints1 = new VectorOfKeyPoint();
			VectorOfKeyPoint keyPoints2 = new VectorOfKeyPoint();
			Mat descriptors1 = new Mat();
			Mat descriptors2 = new Mat();

			akaze.DetectAndCompute(gray1, null, keyPoints1, descriptors1, false);
			akaze.DetectAndCompute(gray2, null, keyPoints2, descriptors2, false);

			using (var matcher = new BFMatcher(DistanceType.L2))
			{
				var matches = new VectorOfVectorOfDMatch();
				matcher.Add(descriptors2); 
				matcher.KnnMatch(descriptors1, matches, 2, null);

				double goodMatches = 0;
				for (int i = 0; i < matches.Size; i++)
				{
					if (matches[i][0].Distance < 0.75 * matches[i][1].Distance) 
					{
						goodMatches++;
					}
				}

				double similarityScore = goodMatches / matches.Size;
				return similarityScore;
			}
		}
	}

}
