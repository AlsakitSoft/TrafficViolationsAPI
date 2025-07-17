using System.ComponentModel.DataAnnotations;

namespace TrafficViolationsAPI.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string? Color { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public UserDto? Owner { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
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
    }
}
