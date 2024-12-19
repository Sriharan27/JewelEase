using JewelEase.Data;
using JewelEase.Models;
using JewelEase.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using OfficeOpenXml;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using iText.Html2pdf;
using JewelEase.Service;
using System.Drawing;


namespace JewelEase.Controllers
{
    public class Admin : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;  // Assuming ApplicationDbContext is your EF DbContext
        private readonly EmailService _emailService;
        private readonly IViewRenderService _viewRenderService;


        public Admin(HttpClient httpClient,ApplicationDbContext context, EmailService emailService, IViewRenderService viewRenderService)
        {
            _httpClient = httpClient;
            _context = context;
            _emailService = emailService;
            _viewRenderService = viewRenderService;
        }
        public IActionResult Index()
        {
            // Customers count where role is "Customer"
            var customersCount = _context.User.Count(u => u.Role == "Customer");

            // Appointments count where status is "Pending"
            var pendingAppointments = _context.Appointments.Count(a => a.Status == "Pending");

            // Total sales (sum of NetTotal)
            var totalSales = Math.Round(_context.Sales.Sum(s => s.NetTotal));

            // Quotations count where status is "Pending"
            var pendingQuotations = _context.ItemQuotation.Count(iq => iq.Status == "Pending");

            // Pass these values to the view
            var dashboardData = new DashboardViewModel
            {
                CustomersCount = customersCount,
                PendingAppointments = pendingAppointments,
                TotalSales = totalSales,
                PendingQuotations = pendingQuotations
            };

            return View(dashboardData);

        }
        public IActionResult QuotationPdf()
        {
            var lineItems = _context.ItemQuotationLineItem
                .Where(item => item.QuotationId == 36)
                .Include(item => item.JewelryItems)
                .Include(item => item.ItemQuotation)
                    .ThenInclude(iq => iq.User) // Include User from ItemQuotation
                .ToList();

            // Find the corresponding ItemQuotation record
            var itemQuotationPdf = lineItems.FirstOrDefault()?.ItemQuotation;
            if (itemQuotationPdf == null)
            {
                return NotFound(); // Handle missing quotation case
            }

            // Prepare data to pass to the PDF view
            var pdfData = new
            {
                QuotationId = itemQuotationPdf.QuotationId,
                UserName = itemQuotationPdf.User?.Name ?? "N/A",  // Confirm this path is valid
                UserEmail = itemQuotationPdf.User?.Email ?? "N/A",
                Items = lineItems.Select(item => new
                {
                    Description = item.JewelryItems?.Name ?? "N/A",
                    Units = item.Qty,
                    PricePerUnit = item.TotalPrice / item.Qty,
                    TotalCost = item.TotalPrice
                }).ToList(),
                Subtotal = itemQuotationPdf.QuotationPrice - itemQuotationPdf.Discount,
                Discount = itemQuotationPdf.Discount,
                NetTotal = itemQuotationPdf.QuotationPrice
            };

            return View(pdfData); // Pass the data to the view
        }

        public async Task<IActionResult> ManageCategory()
        {
            var categories = await _context.Category
                .Where(c => c.Status == "Active")
                .ToListAsync();

            return View(categories); // Pass the data to the view
        }
        public async Task<IActionResult> AddEditCategory(int id, string redirectPage)
        {
            ViewData["RedirectPage"] = redirectPage;

            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category category = await _context.Category.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCategory(Category category, IFormFile CategoryImage, string redirectPage)
        {
            // Remove validations for fields not bound to the model
            ModelState.Remove("ImageUrl");
            ModelState.Remove("CategoryImage");
            ModelState.Remove("redirectPage");

            if (ModelState.IsValid)
            {
                bool isNew = category.CategoryId == 0;

                // If it's a new category, add it to the database
                if (isNew)
                {
                    category.ImageUrl = "ToBeInserted"; // Placeholder until we upload the image
                    _context.Category.Add(category);
                    await _context.SaveChangesAsync(); // Save to get the CategoryId
                }

                // Retrieve the existing category if updating
                Category existingCategory = null;
                if (!isNew)
                {
                    existingCategory = await _context.Category.FindAsync(category.CategoryId);
                    if (existingCategory == null)
                    {
                        TempData["ErrorMessage"] = "Invalid Category";
                        return RedirectToAction(nameof(AddEditCategory), new { redirectPage });
                    }
                    // Update properties except ImageUrl
                    existingCategory.CategoryName = category.CategoryName;
                }

                // If an image is uploaded, process it
                if (CategoryImage != null && CategoryImage.Length > 0)
                {
                    // Validate the file format (allowing only images)
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(CategoryImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Invalid file format. Please upload a valid image.";
                        return RedirectToAction(nameof(AddEditCategory), new { redirectPage });
                    }

                    using (var stream = CategoryImage.OpenReadStream())
                    {
                        using (var image = Image.FromStream(stream))
                        {
                            if (image.Width != 1536 || image.Height != 813)
                            {
                                TempData["ErrorMessage"] = "Image dimensions must be 1536x813.";
                                return RedirectToAction(nameof(AddEditCategory), new { redirectPage });
                            }
                        }
                    }

                    // Set the image folder path
                    string categoryFolder = Path.Combine("wwwroot", "images", "categories");
                    if (!Directory.Exists(categoryFolder))
                    {
                        Directory.CreateDirectory(categoryFolder);
                    }

                    // If updating and the image exists, delete the old image
                    if (!isNew && !string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        string oldImagePath = Path.Combine("wwwroot", existingCategory.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new image with a proper file name
                    string fileName = $"CAT_{category.CategoryName.Substring(0, 3).ToUpper()}_{category.CategoryId}{fileExtension}";
                    string filePath = Path.Combine(categoryFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CategoryImage.CopyToAsync(stream);
                    }

                    // Set the ImageUrl field to the new path
                    category.ImageUrl = Path.Combine("images", "categories", fileName).Replace("\\", "/");

                    // If updating, manually set the ImageUrl in the database
                    if (!isNew)
                    {
                        existingCategory.ImageUrl = category.ImageUrl;
                    }
                }

                // Save or update the category in the database
                if (isNew)
                {
                    TempData["SuccessMessage"] = "Category added successfully!";
                    await _context.SaveChangesAsync(); // Save the new category
                }
                else
                {
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    _context.Entry(existingCategory).State = EntityState.Modified;
                    await _context.SaveChangesAsync(); // Update the existing category
                }
                TempData["SuccessMessage"] = "Category updated successfully!";

                return RedirectToAction(nameof(AddEditCategory), new { redirectPage });
            }

            // Reload categories if validation fails and re-render the view
            return RedirectToAction(nameof(AddEditCategory), new { redirectPage });
        }
        public async Task<IActionResult> DeleteCategory(int Id)
        {
            var category = await _context.Category.FindAsync(Id);
            if (category == null) { return View("Error"); }
            category.Status = "Inactive";
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCategory", "Admin");
        }
        public async Task<IActionResult> ManageJewelryItems()
        {
            // Load all jewelry items, including their associated categories
            var jewelryItems = await _context.JewelryItems
                .Include(j => j.Category) // Ensure to include the Category relationship
                .Where(j=>j.Status == "Active")
                .ToListAsync();

            return View(jewelryItems); // Pass the data to the view
        }
        public async Task<IActionResult> AddEditJewelryItem(int id, string redirectPage)
        {
            ViewData["RedirectPage"] = redirectPage;

            if (id == 0)
            {
                ViewBag.Categories = new SelectList(await _context.Category.ToListAsync(), "CategoryId", "CategoryName");
                return View(new JewelryItems());
            }
            else
            {
                JewelryItems jewelryItem = await _context.JewelryItems.FindAsync(id);
                if (jewelryItem == null)
                {
                    return NotFound();
                }

                ViewBag.Categories = new SelectList(await _context.Category.ToListAsync(), "CategoryId", "CategoryName", jewelryItem.CategoryId);
                return View(jewelryItem);
            }
        }
        // POST: Add or Edit JewelryItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditJewelryItem(JewelryItems jewelryItem, IFormFile ProductImage, string redirectPage)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ProductImage");
            ModelState.Remove("redirectPage");
            ModelState.Remove("Price");
            if (ModelState.IsValid)
            {
                bool isNew = jewelryItem.JewelryItemId == 0;

                // Add new item to the database to get the ID if new
                if (isNew)
                {
                    jewelryItem.ImageUrl = "TobeInserted";
                    _context.JewelryItems.Add(jewelryItem);
                    await _context.SaveChangesAsync();

                    var inventoryHistory = new InventoryHistory
                    {
                        JewelryItemId = jewelryItem.JewelryItemId,
                        Date = DateTime.Now,
                        ChangeAmount = 0,
                        NewStockLevel = jewelryItem.StockLevel,
                        Reason = "New Item"
                    };
                    _context.InventoryHistory.Add(inventoryHistory);
                    await _context.SaveChangesAsync();
                }

                // If updating, get the current jewelry item
                JewelryItems existingJewelryItem = null;
                if (!isNew)
                {
                    existingJewelryItem = await _context.JewelryItems.FindAsync(jewelryItem.JewelryItemId);
                    if (existingJewelryItem == null)
                    {
                        return NotFound();
                    }
                    existingJewelryItem.Name = jewelryItem.Name;
                    existingJewelryItem.CategoryId = jewelryItem.CategoryId;
                    existingJewelryItem.Description = jewelryItem.Description;
                    existingJewelryItem.karats = jewelryItem.karats;
                    existingJewelryItem.IdentificationID = jewelryItem.IdentificationID;
                    existingJewelryItem.TryOnUrl = jewelryItem.TryOnUrl;

                    if (existingJewelryItem.StockLevel != jewelryItem.StockLevel)
                    {
                        var inventoryHistory = new InventoryHistory
                        {
                            JewelryItemId = existingJewelryItem.JewelryItemId,
                            Date = DateTime.Now,
                            ChangeAmount = jewelryItem.StockLevel - existingJewelryItem.StockLevel,
                            NewStockLevel = jewelryItem.StockLevel,
                            Reason = "Update Item"
                        };
                        _context.InventoryHistory.Add(inventoryHistory);
                        await _context.SaveChangesAsync();

                        existingJewelryItem.StockLevel = jewelryItem.StockLevel;
                    }
                }

                // Checking if an image is uploaded
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    var category = await _context.Category.FindAsync(jewelryItem.CategoryId);
                    if (category == null)
                    {
                        TempData["ErrorMessage"] = "Invalid Category";
                        return RedirectToAction(nameof(AddEditJewelryItem), new { redirectPage });
                    }

                    // Validating the file format (allowing only images)
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ProductImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Invalid file format. Please upload an image.";
                        return RedirectToAction(nameof(AddEditJewelryItem), new { redirectPage });
                    }

                    using (var stream = ProductImage.OpenReadStream())
                    {
                        using (var image = Image.FromStream(stream))
                        {
                            if (image.Width != 1600 || image.Height != 1600)
                            {
                                TempData["ErrorMessage"] = "Image dimensions must be 1536x813.";
                                return RedirectToAction(nameof(AddEditJewelryItem), new { redirectPage });
                            }
                        }
                    }

                    // Making the path and file name
                    string categoryFolder = Path.Combine("wwwroot", "images", category.CategoryName);
                    if (!Directory.Exists(categoryFolder))
                    {
                        Directory.CreateDirectory(categoryFolder);
                    }

                    // When editing if the image URL already exists the deleting the old image
                    if (!isNew && !string.IsNullOrEmpty(existingJewelryItem.ImageUrl))
                    {
                        string oldImagePath = Path.Combine("wwwroot", existingJewelryItem.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Saving the new image with the correct format
                    string fileName = $"PRO_{category.CategoryName.Substring(0, 3).ToUpper()}_{jewelryItem.JewelryItemId}{fileExtension}";
                    string filePath = Path.Combine(categoryFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProductImage.CopyToAsync(stream);
                    }

                    // Setting the ImageUrl field to the new path
                    jewelryItem.ImageUrl = Path.Combine("images", category.CategoryName, fileName).Replace("\\", "/");

                    // If editinging, setting manually the ImageUrl in the database
                    if (!isNew)
                    {
                        existingJewelryItem.ImageUrl = jewelryItem.ImageUrl;
                    }
                }

                // Updating or saving the record
                if (isNew)
                {
                    TempData["SuccessMessage"] = "Jewelry added successfully!";
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["SuccessMessage"] = "Jewelry updated successfully!";
                    _context.Entry(existingJewelryItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(AddEditJewelryItem), new { redirectPage });
            }

            // If validation fails, reloading categories and re-rendering the view
            ViewBag.Categories = new SelectList(await _context.Category.ToListAsync(), "CategoryId", "CategoryName", jewelryItem.CategoryId);
            return RedirectToAction(nameof(AddEditJewelryItem), new { redirectPage });
        }
        public async Task<IActionResult> DeleteJewelryItem(int Id)
        {
            var JewelryItem = await _context.JewelryItems.FindAsync(Id);
            if (JewelryItem == null) { return View("Error"); }
            JewelryItem.Status = "Inactive";
            _context.Update(JewelryItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageJewelryItems", "Admin");
        }
           
        public async Task<IActionResult> SalesUpload()
        {
            var salesData = new List<Sales>(); // Initialize the list as an empty list

            // Make a GET request to the Sales API to fetch sales data
            var response = await _httpClient.GetAsync("https://localhost:44372/api/saleapi/sales");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Define the JsonSerializerOptions for camelCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Ensures camelCase mapping
                };

                // Deserialize the content into salesData using the options
                salesData = JsonSerializer.Deserialize<List<Sales>>(content, options);

                // Return the SalesUpload view with the sales data
                return View(salesData);
            }

            // Handle error (e.g., failed API call)
            TempData["ErrorMessage"] = "Failed to load sales data.";
            return View(salesData);
        }

        [HttpPost]
        public async Task<IActionResult> SalesUpload(IFormFile salesFile)
        {
            if (salesFile == null || salesFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please upload a valid file.";
                return RedirectToAction("SalesUpload");
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await salesFile.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync()) // Begin the transaction
                        {
                            try
                            {
                                var salesSheet = package.Workbook.Worksheets[0];
                                var salesData = new List<Sales>();

                                for (int row = 2; row <= salesSheet.Dimension.End.Row; row++)
                                {
                                    var saleRefId = salesSheet.Cells[row, 1].Text;
                                    var email = salesSheet.Cells[row, 2].Text; // Read Email instead of UserId
                                    var name = salesSheet.Cells[row, 3].Text;
                                    if (string.IsNullOrEmpty(saleRefId) ||
                                        string.IsNullOrEmpty(email) ||
                                        string.IsNullOrEmpty(name) ||
                                        string.IsNullOrEmpty(salesSheet.Cells[row, 4].Text) ||  // Discount
                                        string.IsNullOrEmpty(salesSheet.Cells[row, 5].Text) ||  // NetTotal
                                        string.IsNullOrEmpty(salesSheet.Cells[row, 6].Text))    // SaleDate
                                    {
                                        continue;  // Skip empty or incomplete rows
                                    }

                                    // Check if the user exists by email
                                    var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
                                    if (user == null)
                                    {
                                        var randomPassword = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 8)
                                          .Select(s => s[new Random().Next(s.Length)]).ToArray());

                                        user = new User
                                        {
                                            Email = email,
                                            PasswordHash = randomPassword,  // Default password
                                            Role = "Customer",  // Default role
                                            Name = name  // Empty name
                                        };

                                        _context.User.Add(user);
                                        await _context.SaveChangesAsync(); // Save to get the new UserId

                                        var message = $"JewelEase\n\nWelcome {user.Name}\n\nYour Username : {user.Email}\n\nYour Password : {user.PasswordHash}\n\nDo not share with anyone, please reset password after logging in!";
                                        var emailBody = message;

                                        // Send the OTP email
                                        await _emailService.SendEmailAsync(user.Email, "New Account Created", emailBody);
                                    }

                                    var sale = new Sales
                                    {
                                        SaleRefId = saleRefId,
                                        UserId = user.UserId, // Use the retrieved or new UserId
                                        Discount = Convert.ToInt32(salesSheet.Cells[row, 4].Value),
                                        NetTotal = Convert.ToDecimal(salesSheet.Cells[row, 5].Value),
                                        SaleDate = DateTime.Parse(salesSheet.Cells[row, 6].Text)
                                    };
                                    salesData.Add(sale);
                                }

                                if (salesData.Any())  // Only add if there are valid records
                                {
                                    _context.Sales.AddRange(salesData);
                                    await _context.SaveChangesAsync();
                                }

                                // Read Sheet2 into SalesLineItem table
                                var lineItemsSheet = package.Workbook.Worksheets[1];
                                var lineItemsData = new List<SalesLineItem>();

                                for (int row = 2; row <= lineItemsSheet.Dimension.End.Row; row++)
                                {
                                    var saleRefId = lineItemsSheet.Cells[row, 1].Text;
                                    var jewelleryrefid = lineItemsSheet.Cells[row, 2].Text;
                                    if (string.IsNullOrEmpty(saleRefId) ||
                                        string.IsNullOrEmpty(jewelleryrefid) ||  // JewelryItemId
                                        string.IsNullOrEmpty(lineItemsSheet.Cells[row, 3].Text) ||  // Qty
                                        string.IsNullOrEmpty(lineItemsSheet.Cells[row, 4].Text))    // TotalPrice
                                    {
                                        continue;  // Skip empty or incomplete rows
                                    }

                                    var sale = salesData.FirstOrDefault(s => s.SaleRefId == saleRefId); // Find the matching sale by SaleRefId

                                    var jewelry = await _context.JewelryItems.FirstOrDefaultAsync(u => u.IdentificationID == jewelleryrefid);

                                    if (jewelry == null)
                                    {
                                        // Store the message in TempData about the missing jewelry reference
                                        TempData["ErrorMessage"] = $"JewelryRefId '{jewelleryrefid}' for SaleRefId '{saleRefId}' was not found in the database.";
                                        await transaction.RollbackAsync(); // Rollback the transaction
                                        return RedirectToAction("SalesUpload");  // Exit the function and redirect
                                    }

                                    var saleQty = Convert.ToInt32(lineItemsSheet.Cells[row, 3].Value);

                                    // Check if the sale quantity is greater than stock level
                                    if (saleQty > jewelry.StockLevel)
                                    {
                                        // Store the error message in TempData and exit the function
                                        TempData["ErrorMessage"] = $"SaleRefId '{saleRefId}' for JewelryRefId '{jewelleryrefid}' has a quantity ({saleQty}) greater than stock level ({jewelry.StockLevel}).";
                                        await transaction.RollbackAsync(); // Rollback the transaction
                                        return RedirectToAction("SalesUpload");  // Exit the function and redirect
                                    }

                                    if (sale != null)
                                    {
                                        var lineItem = new SalesLineItem
                                        {
                                            SaleId = sale.SaleId,  // Use the generated SaleId
                                            JewelryItemId = jewelry.JewelryItemId,
                                            Qty = saleQty,
                                            TotalPrice = Convert.ToDecimal(lineItemsSheet.Cells[row, 4].Value)
                                        };
                                        lineItemsData.Add(lineItem);

                                        // Subtract the sale quantity from the stock level
                                        jewelry.StockLevel -= saleQty;
                                        _context.JewelryItems.Update(jewelry); // Update the stock level in the database
                                    }
                                }

                                if (lineItemsData.Any())  // Only add if there are valid records
                                {
                                    _context.SalesLineItem.AddRange(lineItemsData);
                                    await _context.SaveChangesAsync();
                                }

                                await transaction.CommitAsync(); // Commit the transaction if everything is successful
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync(); // Rollback the transaction in case of an error
                                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                                return RedirectToAction("SalesUpload");
                            }
                        }
                    }
                }

                TempData["SuccessMessage"] = "Sales data uploaded successfully.";
                return RedirectToAction("SalesUpload");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("SalesUpload");
            }
        }

        public async Task<IActionResult> SalesLineItemView(int id)
        {
            var salesLineItems = new List<SalesLineItem>(); // Initialize the list as an empty list

            // Make a GET request to the SalesLineItems API to fetch sales line items related to the SaleId (id)
            var response = await _httpClient.GetAsync($"https://localhost:44372/api/saleapi/saleslineitems/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Ensures camelCase mapping
                };

                salesLineItems = JsonSerializer.Deserialize<List<SalesLineItem>>(content, options);

                // Return the SalesLineItemView with the sales line items data
                return View(salesLineItems);
            }

            // Handle error (e.g., failed API call)
            TempData["ErrorMessage"] = "Failed to load sales line items.";
            return View(salesLineItems);
        }
        // Endpoint to fetch sales data grouped by category
        public JsonResult GetSalesData()
        {
            var salesData = _context.SalesLineItem
                .Include(sli => sli.JewelryItems)
                .ThenInclude(ji => ji.Category) // Assuming Category is a field in JewelryItems
                .GroupBy(sli => sli.JewelryItems.Category.CategoryName) // Group by category name
                .Select(g => new
                {
                    Category = g.Key,
                    TotalSales = g.Sum(sli => sli.TotalPrice),
                    SaleDates = g.Select(sli => sli.Sales.SaleDate).ToList()
                })
                .ToList();

            return Json(salesData);
        }
        public IActionResult DownloadSalesTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "doc", "SalesTemplate.xlsx");

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            string fileName = "SalesTemplate.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        public async Task<IActionResult> ManageQuotations()
        {
            var quotations = await _context.ItemQuotation
                .Include(q => q.User)
                .ToListAsync();

            return View(quotations); // Pass the data to the view
        }
        public async Task<IActionResult> QuotationLineItemView(int id)
        {
            var quotationLineItems = await _context.ItemQuotationLineItem
                .Include(q => q.JewelryItems)
                .Where(q => q.QuotationId == id)
                .ToListAsync();

            return View(quotationLineItems);
        }
        public async Task<IActionResult> ManageAppointments()
        {
            var appointments = await _context.Appointments
                .Include(q => q.User)
                .ToListAsync();

            return View(appointments); // Pass the data to the view
        }
        public async Task<IActionResult> SendAppointmentInvitation(int id)
        {
            var appointments = await _context.Appointments
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

            return View(appointments);
        }
        public async Task<IActionResult> SendAppointmentInvite(Appointments appointment)
        {
            ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                var appointmentDetail = await _context.Appointments
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (appointmentDetail == null)
                {
                    TempData["ErrorMessage"] = "Appintment Not Found";

                    return View(appointmentDetail);
                }
                else
                {
                    appointmentDetail.InvitationBody = appointment.InvitationBody;
                    appointmentDetail.Status = "Completed";

                    _context.Appointments.Update(appointmentDetail);
                    _context.SaveChanges();

                    var emailBody = appointment.InvitationBody;

                    // Send the OTP email
                    await _emailService.SendEmailAsync(appointmentDetail.User.Email, "JewelEase Appointment Invitation", emailBody);

                    TempData["SuccessMessage"] = "Invite Send Successfully.";

                    return RedirectToAction("ManageAppointments", "Admin");
                }
            }
            return View(appointment);
        }
        public async Task<IActionResult> ViewInvitation(int id)
        {
            var appointments = await _context.Appointments
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

            return View(appointments);
        }
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

/*        // Action to handle the file upload
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileModel = new FileModel
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileData = memoryStream.ToArray()
                    };

                    // Save the fileModel to the database
                    _context.FileModel.Add(fileModel);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = "File uploaded successfully!";
                    return View("Upload");
                }
            }
            ViewBag.Message = "Please select a valid file.";
            return View("Upload");
        }
        // Retrieve and serve the GLB file from the database
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await _context.FileModel.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            return File(file.FileData, file.ContentType, file.FileName);
        }*/
        public IActionResult SendQuotationView(int id) // 'id' is the quotation ID
        {
            // Fetch the line items for the given quotation ID
            var lineItems = _context.ItemQuotationLineItem
                .Include(i => i.JewelryItems) // Include related jewelry items if needed
                .ThenInclude(i => i.Category) // Include the category of the jewelry items
                .Where(i => i.QuotationId == id) // Adjust this if the property is different
                .ToList();

            // Check if any line items exist
            if (lineItems == null || !lineItems.Any())
            {
                // Handle the case where no line items were found (e.g., return an error message)
                return NotFound("No line items found for this quotation.");
            }

            // Pass the list of line items to the view
            return View(lineItems);
        }
        [HttpPost]
        public async Task<IActionResult> SendQuotationEstimate(ItemQuotationUpdateModel model)
        {
            ModelState.Remove("Status");

            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update ItemQuotation
            var itemQuotation = _context.ItemQuotation.Find(model.QuotationId);
            if (itemQuotation == null)
            {
                return NotFound();
            }

            itemQuotation.Discount = model.TotalDiscount; // Set the new discount
            itemQuotation.QuotationPrice = model.NetTotal; // Set the new quotation price
            itemQuotation.SentDate = DateTime.Now;
            itemQuotation.Status = "Completed";
            _context.ItemQuotation.Update(itemQuotation);

            // Update ItemQuotationLineItems
            foreach (var lineItem in model.ItemQuotationLineItems)
            {
                var existingLineItem = _context.ItemQuotationLineItem.Find(lineItem.QuotationLineItemId);
                if (existingLineItem != null)
                {
                    existingLineItem.TotalPrice = lineItem.TotalPrice; // Update the total price
                    _context.ItemQuotationLineItem.Update(existingLineItem);
                }
            }

            // Save all changes to the database
            _context.SaveChanges();

            // Fetch line items including related JewelryItems and User details from ItemQuotation
            var lineItems = _context.ItemQuotationLineItem
                .Where(item => item.QuotationId == model.QuotationId)
                .Include(item => item.JewelryItems)
                .Include(item => item.ItemQuotation)
                    .ThenInclude(iq => iq.User) // Include User from ItemQuotation
                .ToList();

            // Find the corresponding ItemQuotation record
            var itemQuotationPdf = lineItems.FirstOrDefault()?.ItemQuotation;
            if (itemQuotationPdf == null)
            {
                return NotFound(); // Handle missing quotation case
            }

            // Prepare data to pass to the PDF view
            var pdfData = new
            {
                QuotationId = itemQuotation.QuotationId,
                UserName = itemQuotation.User?.Name ?? "N/A",  // Confirm this path is valid
                UserEmail = itemQuotation.User.Email,
                Items = lineItems.Select(item => new
                {
                    Description = item.JewelryItems?.Name ?? "N/A",
                    Units = item.Qty,
                    PricePerUnit = item.TotalPrice / item.Qty,
                    TotalCost = item.TotalPrice
                }).ToList(),
                Subtotal = itemQuotation.QuotationPrice + itemQuotation.Discount,
                Discount = itemQuotation.Discount,
                NetTotal = itemQuotation.QuotationPrice
            };

            // Render the view as HTML
            var viewHtml = await _viewRenderService.RenderViewToStringAsync("Admin/QuotationPdf", pdfData);

            // Convert the HTML to PDF using iText7
            byte[] pdfBytes;
            using (MemoryStream pdfStream = new MemoryStream())
            {
                ConverterProperties converterProperties = new ConverterProperties();
                HtmlConverter.ConvertToPdf(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(viewHtml)), pdfStream, converterProperties);
                pdfBytes = pdfStream.ToArray();
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ItemQuotations");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = $"Quo_{model.QuotationId}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            TempData["SuccessMessage"] = "Quotation PDF generated and sent successfully.";

            var emailBody = $"Dear {itemQuotation.User?.Name},\n\nAttached is your quotation. Please review the details and contact us if you have any questions.\n\nBest regards,\nJewelEase";

            await _emailService.SendEmailAsync(itemQuotation.User.Email,"Your Quotation from JewelEase",emailBody,pdfBytes,fileName);

            return RedirectToAction("ManageQuotations");
        }
        public async Task<IActionResult> ViewQuotation(int id)
        {
            // Define the path to the folder containing the PDF files
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ItemQuotations");

            // Define the full file path based on the provided id
            var fileName = $"Quo_{id}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Return a 404 if the file is not found
            }

            // Read the file as a stream
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Create and return a FileStreamResult with the inline content disposition
            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return new FileStreamResult(fileStream, "application/pdf");
        }
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();

			// Redirect to the home page after logging out
			return Redirect("/");
		}
	}
	// Model to hold the update data
	public class ItemQuotationUpdateModel
    {
        public int QuotationId { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTotal { get; set; }
        public List<ItemQuotationLineItemUpdateModel> ItemQuotationLineItems { get; set; }
    }

    public class ItemQuotationLineItemUpdateModel
    {
        public int QuotationLineItemId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
