using System.ComponentModel.DataAnnotations;

namespace TrafficViolationsAPI.DTOs
{
    public class ViolationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal FineAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Guid VehicleId { get; set; }
        public Guid OfficerId { get; set; }
        public string? EvidenceImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public VehicleDto? Vehicle { get; set; }
        public UserDto? Officer { get; set; }
    }

    public class CreateViolationDto
    {
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
        [Range(0.01, double.MaxValue, ErrorMessage = "Fine amount must be greater than 0")]
        public decimal FineAmount { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid OfficerId { get; set; }

        [MaxLength(500)]
        public string? EvidenceImageUrl { get; set; }
    }

    public class UpdateViolationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Paid, Cancelled
    }
}