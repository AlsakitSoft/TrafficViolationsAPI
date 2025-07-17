using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficViolationsAPI.Models
{
    public class Violation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

        public DateTime? PaymentDate { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid OfficerId { get; set; }

        [MaxLength(500)]
        public string? EvidenceImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Property
        [NotMapped]
        public bool IsPaid => Status == "Paid";

        // Navigation Properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        [ForeignKey("OfficerId")]
        public virtual User Officer { get; set; } = null!;
    }
}