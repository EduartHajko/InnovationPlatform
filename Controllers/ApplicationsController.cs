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