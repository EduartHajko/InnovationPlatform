using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InnovationPlatform.Data;
using InnovationPlatform.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace InnovationPlatform.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Email and password are required.";
                    return View();
                }

                // Find user by email or username
                var user = await _context.SimpleUsers
                    .FirstOrDefaultAsync(u => (u.Email == email || u.Username == email) && u.IsActive);

                if (user == null || user.Password != password)
                {
                    ViewBag.Error = "Invalid login attempt.";
                    return View();
                }

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect based on role
                if (user.Role == "Executive")
                {
                    return RedirectToAction("ExecutiveDashboard", "Applications");
                }
                else if (user.Role == "Expert")
                {
                    return RedirectToAction("ExpertDashboard", "Applications");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Login failed: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "All fields are required.";
                    return View();
                }

                if (password != confirmPassword)
                {
                    ViewBag.Error = "Passwords do not match.";
                    return View();
                }

                if (password.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View();
                }

                // Check if user already exists
                var existingUser = await _context.SimpleUsers
                    .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

                if (existingUser != null)
                {
                    ViewBag.Error = "A user with this email or username already exists.";
                    return View();
                }

                // Create new user
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    Password = password, // In production, this should be hashed
                    Role = "Applicant", // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SimpleUsers.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Registration failed: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
