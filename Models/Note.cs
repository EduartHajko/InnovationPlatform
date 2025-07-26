using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovationPlatform.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string NoteText { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int ApplicationId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
