using JewelEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
	private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
		var sliders = _context.Slider.OrderBy(s => s.Order).ToList();
		return View(sliders);
    }
	public IActionResult Index1()
	{
		return View();
	}

	public IActionResult Item()
    {
        return View();
    }

    public IActionResult Category()
    {
        return View();
    }
	public IActionResult Products()
	{
		return View();
	}
	public IActionResult ShoppingCart()
	{
		return View();
	}
	public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
    public IActionResult ForgotPassword()
    {
        return View();
    }
	public IActionResult About()
	{
		return View();
	}
	public IActionResult Contact()
	{
        return View();
	}
	public IActionResult tempview()
	{
		return View();
	}
	public IActionResult Cart()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Home");
        }

        // Fetch and display user-specific data
        return View();
    }

}
