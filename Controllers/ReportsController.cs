using Microsoft.AspNetCore.Mvc;

namespace JewelEase.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Sales()
        {
            return View();
        }

        public IActionResult StockPrediction()
        {
            return View();
        }
    }
}
