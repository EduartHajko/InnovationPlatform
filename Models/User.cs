namespace InnovationPlatform.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<Application> AssignedApplications { get; set; } = new List<Application>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
