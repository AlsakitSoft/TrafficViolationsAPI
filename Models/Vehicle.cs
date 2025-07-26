using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TrafficViolationsAPI.Models
{
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        public int? Year { get; set; }

        [MaxLength(30)]
        public string? Color { get; set; }

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string VehicleType { get; set; } = string.Empty;

        [Required]
        public Guid OwnerId { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}