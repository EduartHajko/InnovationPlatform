using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovationPlatform.Models
{
    public class Application
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AgeGroup { get; set; } = string.Empty; // "15-18", "19-24", "25-29"

        [Required]
        [StringLength(100)]
        public string Municipality { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PrototypeUrl { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "I Ri"; // I Ri, Në Progres, Në Mentorim, Në Prezantim, Në Implementim, Zbatuar

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int? UserId { get; set; }
        public int CategoryId { get; set; }
        public int? AssignedExpertId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("AssignedExpertId")]
        public virtual User? AssignedExpert { get; set; }

        public virtual ICollection<ApplicationFile> Files { get; set; } = new List<ApplicationFile>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
