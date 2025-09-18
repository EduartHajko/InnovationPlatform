// Updated section of code

// GET: Applications/Submit
public async Task<IActionResult> Submit()
{
    ViewBag.Applications = await _context.Categories.ToListAsync(); // Changed from ViewBag.Categories to ViewBag.Applications
    return View();
}

// POST: Applications/Submit
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(Application application, List<IFormFile> files)
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(application.Title) ||
            string.IsNullOrWhiteSpace(application.Description) ||
            application.CategoryId == 0 ||
            string.IsNullOrWhiteSpace(application.AgeGroup) ||
            string.IsNullOrWhiteSpace(application.Municipality))
        {
            TempData["Error"] = "Të gjitha fushat e detyrueshme duhet të plotësohen.";
            ViewBag.Applications = await _context.Categories.ToListAsync(); // Changed from ViewBag.Categories to ViewBag.Applications
            return View(application);
        }

        if (application.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 100)
        {
            TempData["Error"] = "Përshkrimi duhet të ketë të paktën 100 fjalë.";
            ViewBag.Applications = await _context.Categories.ToListAsync(); // Changed from ViewBag.Categories to ViewBag.Applications
            return View(application);
        }

        // Set user if authenticated using simple auth
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                application.UserId = userId;
            }
        }

        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        application.Status = "I Ri";

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // Handle file uploads
        if (files != null && files.Count > 0)
        {
            await HandleFileUploads(files, application.Id);
        }

        TempData["Success"] = "Aplikimi juaj u dorëzua me sukses!";

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("MyApplications");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
    catch (Exception)
    {
        TempData["Error"] = "Ndodhi një gabim gjatë dorëzimit të aplikimit.";
        ViewBag.Applications = await _context.Categories.ToListAsync(); // Changed from ViewBag.Categories to ViewBag.Applications
        return View(application);
    }
}

// GET: Applications/ExecutiveDashboard
[Authorize]
public async Task<IActionResult> ExecutiveDashboard()
{
    // Check if user has Executive role from claims (simple auth)
    if (!User.IsInRole("Executive"))
    {
        TempData["Error"] = "Nuk keni akses në këtë faqe.";
        return RedirectToAction("Index", "Home");
    }

    // KPI calculations
    var totalApplications = await _context.Applications.CountAsync();

    var ageGroups = await _context.Applications
        .GroupBy(a => a.AgeGroup)
        .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
        .ToListAsync();

    var categories = await _context.Applications
        .Include(a => a.Category)
        .GroupBy(a => a.Category.Name)
        .Select(g => new { Category = g.Key, Count = g.Count() })
        .ToListAsync();

    var municipalities = await _context.Applications
        .GroupBy(a => a.Municipality)
        .Select(g => new { Municipality = g.Key, Count = g.Count() })
        .ToListAsync();

    var statuses = await _context.Applications
        .GroupBy(a => a.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToListAsync();

    ViewBag.TotalApplications = totalApplications;
    ViewBag.AgeGroups = ageGroups;
    ViewBag.Applications = categories; // Changed from ViewBag.Categories to ViewBag.Applications
    ViewBag.Municipalities = municipalities;
    ViewBag.Statuses = statuses;

    return View();
}