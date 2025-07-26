using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovationPlatform.Models
{
    public class ApplicationFile
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string OriginalName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        // Foreign Key
        public int ApplicationId { get; set; }
        
        // Navigation property
        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; } = null!;
    }
}
