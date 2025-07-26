using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficViolationsAPI.Models
{
    public class ViolationType
    {
        [Key]
        public Guid Violation_Type_ID { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Violation_Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultFineAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
