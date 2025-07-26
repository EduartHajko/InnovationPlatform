using System.ComponentModel.DataAnnotations;

namespace InnovationPlatform.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
