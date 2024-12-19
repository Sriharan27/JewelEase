using JewelEase.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JewelEase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace JewelEase.Controllers
{
    [Route("api/index")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly EmailService _emailService;
        private static readonly Dictionary<string, string> OtpStorage = new();
        // Mock data for demonstration purposes
        private readonly ApplicationDbContext _context;

        public IndexController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("Pro")]
        public async Task<ActionResult<IEnumerable<JewelryItems>>> GetJewelryItems()
        {
            return await _context.JewelryItems.ToListAsync();
        }

        [HttpGet("Pro/{proID}")]
        public async Task<ActionResult<JewelryItems>> GetJewelryItemByproID(int proID)
        {
            var jewelryItem = await _context.JewelryItems.FindAsync(proID);

            if (jewelryItem == null)
            {
                return NotFound();
            }

            return jewelryItem;
        }

        [HttpGet("Cat/{catID}")]
        public async Task<ActionResult<Category>> GetCategoryByCatId(int catID)
        {
            var CategoryItem = await _context.Category.FindAsync(catID);

            if (CategoryItem == null)
            {
                return NotFound();
            }

            return CategoryItem;
        }

        [HttpGet("ProByCategory/{catID}")]
        public async Task<ActionResult<IEnumerable<JewelryItems>>> GetJewelryItemsByCategory(int catID)
        {
            var items = await _context.JewelryItems
                                      .Where(j => j.CategoryId == catID)
                                      .ToListAsync();
            return Ok(items);
        }

        [HttpGet("Cat")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            return await _context.Category.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JewelryItems>> GetJewelryItem(int id)
        {
            var jewelryItem = await _context.JewelryItems.FindAsync(id);

            if (jewelryItem == null)
            {
                return NotFound();
            }

            return jewelryItem;
        }

        // GET: api/index/search?name=diamond
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JewelryItems>>> SearchJewelryItems(string name)
        {
            var items = await _context.JewelryItems
                                      .Where(j => j.Name.Contains(name))
                                      .ToListAsync();
            return Ok(items);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVerification loginRequest1)
        {
            if (string.IsNullOrEmpty(loginRequest1.Username) || string.IsNullOrEmpty(loginRequest1.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _context.User
            .SingleOrDefaultAsync(u => u.Email == loginRequest1.Username);

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest1.Password, user.PasswordHash);

            if (user == null || !isPasswordValid)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Create a session and set a cookie
            HttpContext.Session.SetString("UserEmail", user.Email);
			HttpContext.Session.SetInt32("UserId", user.UserId);
			HttpContext.Session.SetString("UserName", user.Name);
			HttpContext.Session.SetString("UserRole", user.Role);

			return Ok(new { status = "Success", role = user.Role });
		}

		// POST: api/index/register
		[HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] User newUser)
        {
            // Check if email is already registered
            var existingEmail = await _context.User.SingleOrDefaultAsync(u => u.Email == newUser.Email);
            if (existingEmail != null)
            {
                return BadRequest("Email is already registered.");
            }

            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);
            // Add new user to the database
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
        }

        // GET: api/index/user/{id}
        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        // GET: api/index/user/{email}
        [HttpGet("GetUserByEmail/{email}")]
        public async Task<ActionResult<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound();
            }
            return user;
        }
        [HttpPost("SendOtp")]
        public async Task<IActionResult> SendOtp([FromBody] Email model)
        {
            var email = model.email;

            // Call GetUserByEmailAsync to check if the user exists
            var userResult = await GetUserByEmailAsync(email);

            // Check if the user is not found (userResult will be NotFound in this case)
            if (userResult.Result is NotFoundResult)
            {
                // If user does not exist and formtype is ResetPassword
                if (model.formtype == "ResetPassword")
                {
                    return new JsonResult(new { success = false, errorMessage = "User does not exist" });
                }
            }
            else
            {
                // If user is found
                if (model.formtype == "Signup")
                {
                    return new JsonResult(new { success = false, errorMessage = "User already exists! Forgot password? Try reset password" });
                }
            }

            // Generate OTP
            var otp = new Random().Next(1000, 9999).ToString();

            OtpStorage[model.email] = otp;

            // Create the message body
            var message = $"JewelEase\n\nYour OTP is: {otp}\n\nDo not share with anyone!";
            var emailBody = message;

            // Send the OTP email
            await _emailService.SendEmailAsync(email, "JewelEase Reset Password", emailBody);

            // If the email was sent successfully
            return new JsonResult(new { success = true, otp = otp });
        }


        [HttpPost("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] OtpModel model)
        {
            if (OtpStorage.TryGetValue(model.Email, out var storedOtp) && storedOtp == model.Otp)
            {
                OtpStorage.Remove(model.Email);
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }
        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetData)
        {
            if (resetData == null || string.IsNullOrEmpty(resetData.Email) || string.IsNullOrEmpty(resetData.NewPassword))
            {
                return BadRequest("Invalid data.");
            }

            var actionResult = await GetUserByEmailAsync(resetData.Email);

            if (actionResult == null)
            {
                return NotFound("User not found.");
            }

            var user = actionResult.Value;

            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetData.NewPassword);

                // Save the updated user data
                _context.User.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Password reset successful.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
		[HttpPost]
		[Route("Logout")]
		public IActionResult Logout()
		{
			// Clear the session
			HttpContext.Session.Clear(); // This will remove all session variables

			// Optionally, you can return a response
			return Ok(new { message = "Logout successful" });
		}
	}
    public class Email
    {
        public string email { get; set; }
        public string formtype { get; set; }
    }

    public class OtpModel
    {
        public string Otp { get; set; }
        public string Email { get; set; }
    }
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

}

