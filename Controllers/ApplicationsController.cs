using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InnovationPlatform.Data;
using InnovationPlatform.Models;

namespace InnovationPlatform.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ApplicationsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Applications/Submit
        public async Task<IActionResult> Submit()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
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
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View(application);
                }

                if (application.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 100)
                {
                    TempData["Error"] = "Përshkrimi duhet të ketë të paktën 100 fjalë.";
                    ViewBag.Categories = await _context.Categories.ToListAsync();
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
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(application);
            }
        }

        // GET: Applications/MyApplications
        [Authorize]
        public async Task<IActionResult> MyApplications()
        {
            // Check role from claims (simple auth)
            if (User.IsInRole("Executive"))
            {
                return RedirectToAction("ExecutiveDashboard");
            }
            else if (User.IsInRole("Expert"))
            {
                return RedirectToAction("ExpertDashboard");
            }

            // Get current user's applications
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var applications = new List<Application>();

            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                applications = await _context.Applications
                    .Include(a => a.Category)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }

            return View(applications);
        }

        // GET: Applications/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Category)
                .Include(a => a.User)
                .Include(a => a.AssignedExpert)
                .Include(a => a.Files)
                .Include(a => a.Notes)
                .ThenInclude(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            // For simple auth, we'll use role-based access control from claims
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Basic permission check - in a full implementation you'd check user ID against application
            if (userRole == "Applicant")
            {
                // For now, allow access - in full implementation check if user owns the application
            }
            else if (userRole == "Expert")
            {
                // For now, allow access - in full implementation check if expert is assigned
            }

            return View(application);
        }

        // GET: Applications/ExpertDashboard
        [Authorize]
        public async Task<IActionResult> ExpertDashboard()
        {
            // Check if user has Expert role from claims (simple auth)
            if (!User.IsInRole("Expert"))
            {
                TempData["Error"] = "Nuk keni akses në këtë faqe.";
                return RedirectToAction("Index", "Home");
            }

            // Get current expert's ID
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var applications = new List<Application>();

            if (userIdClaim != null && int.TryParse(userIdClaim, out int expertId))
            {
                // Get applications assigned to this expert
                applications = await _context.Applications
                    .Include(a => a.Category)
                    .Include(a => a.User)
                    .Include(a => a.Files)
                    .Where(a => a.AssignedExpertId == expertId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }

            return View(applications);
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
            ViewBag.Categories = categories;
            ViewBag.Municipalities = municipalities;
            ViewBag.Statuses = statuses;

            return View();
        }

        // GET: Applications/ExecutiveApplications
        [Authorize]
        public async Task<IActionResult> ExecutiveApplications()
        {
            // Check if user has Executive role from claims (simple auth)
            if (!User.IsInRole("Executive"))
            {
                TempData["Error"] = "Nuk keni akses në këtë faqe.";
                return RedirectToAction("Index", "Home");
            }

            var applications = await _context.Applications
                .Include(a => a.Category)
                .Include(a => a.User)
                .Include(a => a.AssignedExpert)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var experts = await _context.SimpleUsers
                .Where(u => u.Role == "Expert")
                .ToListAsync();

            ViewBag.Experts = experts;

            return View(applications);
        }

        private async Task HandleFileUploads(List<IFormFile> files, int applicationId)
        {
            var allowedExtensions = new[] { ".doc", ".docx", ".mov", ".ppt", ".pptx", ".pdf", ".txt" };
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            int fileCount = 0;
            foreach (var file in files)
            {
                if (file.Length > 0 && fileCount < 5)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (allowedExtensions.Contains(extension))
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var applicationFile = new ApplicationFile
                        {
                            FileName = uniqueFileName,
                            OriginalName = file.FileName,
                            FilePath = filePath,
                            FileType = extension.TrimStart('.'),
                            ApplicationId = applicationId,
                            UploadDate = DateTime.UtcNow
                        };

                        _context.ApplicationFiles.Add(applicationFile);
                        fileCount++;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // POST: Applications/UpdateStatus
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                var application = await _context.Applications.FindAsync(applicationId);
                if (application == null)
                {
                    return Json(new { success = false, message = "Aplikimi nuk u gjet." });
                }

                application.Status = status;
                application.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Statusi u përditësua me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/AssignExpert
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignExpert(int applicationId, int? expertId)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                var application = await _context.Applications.FindAsync(applicationId);
                if (application == null)
                {
                    return Json(new { success = false, message = "Aplikimi nuk u gjet." });
                }

                // Verify expert exists if expertId is provided
                if (expertId.HasValue)
                {
                    var expert = await _context.SimpleUsers.FindAsync(expertId.Value);
                    if (expert == null || expert.Role != "Expert")
                    {
                        return Json(new { success = false, message = "Eksperti nuk u gjet." });
                    }
                }

                application.AssignedExpertId = expertId;
                application.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Eksperti u caktua me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/BulkAssignExpert
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BulkAssignExpert(List<int> applicationIds, int expertId)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                // Verify expert exists
                var expert = await _context.SimpleUsers.FindAsync(expertId);
                if (expert == null || expert.Role != "Expert")
                {
                    return Json(new { success = false, message = "Eksperti nuk u gjet." });
                }

                var applications = await _context.Applications
                    .Where(a => applicationIds.Contains(a.Id))
                    .ToListAsync();

                foreach (var application in applications)
                {
                    application.AssignedExpertId = expertId;
                    application.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{applications.Count} aplikime u caktuan me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/AddNote
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNote(int applicationId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Shënimi nuk mund të jetë bosh." });
                }

                var application = await _context.Applications.FindAsync(applicationId);
                if (application == null)
                {
                    return Json(new { success = false, message = "Aplikimi nuk u gjet." });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Json(new { success = false, message = "Gabim në identifikimin e përdoruesit." });
                }

                var note = new Note
                {
                    ApplicationId = applicationId,
                    UserId = userId,
                    NoteText = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Shënimi u shtua me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/Delete
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                var application = await _context.Applications
                    .Include(a => a.Files)
                    .Include(a => a.Notes)
                    .FirstOrDefaultAsync(a => a.Id == applicationId);

                if (application == null)
                {
                    return Json(new { success = false, message = "Aplikimi nuk u gjet." });
                }

                // Delete associated files from filesystem
                foreach (var file in application.Files)
                {
                    if (System.IO.File.Exists(file.FilePath))
                    {
                        System.IO.File.Delete(file.FilePath);
                    }
                }

                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Aplikimi u fshi me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // GET: Applications/ManageExperts
        [Authorize]
        public async Task<IActionResult> ManageExperts()
        {
            // Check if user has Executive role from claims (simple auth)
            if (!User.IsInRole("Executive"))
            {
                TempData["Error"] = "Nuk keni akses në këtë faqe.";
                return RedirectToAction("Index", "Home");
            }

            var experts = await _context.SimpleUsers
                .Where(u => u.Role == "Expert")
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(experts);
        }

        // POST: Applications/AddExpert
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddExpert(string username, string email, string password)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                // Validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Json(new { success = false, message = "Të gjitha fushat janë të detyrueshme." });
                }

                if (password.Length < 6)
                {
                    return Json(new { success = false, message = "Fjalëkalimi duhet të ketë të paktën 6 karaktere." });
                }

                // Check if user already exists
                var existingUser = await _context.SimpleUsers
                    .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Një përdorues me këtë email ose emër përdoruesi ekziston tashmë." });
                }

                // Create new expert
                var newExpert = new User
                {
                    Username = username,
                    Email = email,
                    Password = password, // In production, this should be hashed
                    Role = "Expert",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SimpleUsers.Add(newExpert);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Eksperti u shtua me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/ToggleExpertStatus
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleExpertStatus(int expertId)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                var expert = await _context.SimpleUsers.FindAsync(expertId);
                if (expert == null || expert.Role != "Expert")
                {
                    return Json(new { success = false, message = "Eksperti nuk u gjet." });
                }

                expert.IsActive = !expert.IsActive;
                await _context.SaveChangesAsync();

                string status = expert.IsActive ? "aktivizua" : "çaktivizua";
                return Json(new { success = true, message = $"Eksperti u {status} me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }

        // POST: Applications/DeleteExpert
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteExpert(int expertId)
        {
            try
            {
                if (!User.IsInRole("Executive"))
                {
                    return Json(new { success = false, message = "Nuk keni akses për këtë veprim." });
                }

                var expert = await _context.SimpleUsers.FindAsync(expertId);
                if (expert == null || expert.Role != "Expert")
                {
                    return Json(new { success = false, message = "Eksperti nuk u gjet." });
                }

                // Check if expert is assigned to any applications
                var assignedApplications = await _context.Applications
                    .Where(a => a.AssignedExpertId == expertId)
                    .CountAsync();

                if (assignedApplications > 0)
                {
                    return Json(new { success = false, message = $"Eksperti nuk mund të fshihet sepse është caktuar në {assignedApplications} aplikime." });
                }

                _context.SimpleUsers.Remove(expert);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Eksperti u fshi me sukses!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Gabim: {ex.Message}" });
            }
        }
    }
}
