using Microsoft.AspNetCore.Identity;

namespace InnovationPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; } = "Applicant"; // Applicant, Expert, Executive
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<Application> AssignedApplications { get; set; } = new List<Application>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
