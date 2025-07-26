//using System.ComponentModel.DataAnnotations;

//namespace TrafficViolationsAPI.DTOs
//{
//    public class ViolationDto
//    {
//        public Guid Id { get; set; }
//        public string Type { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public string Location { get; set; } = string.Empty;
//        public DateTime Timestamp { get; set; }
//        public decimal FineAmount { get; set; }
//        public string Status { get; set; } = string.Empty;
//        public bool IsPaid { get; set; }
//        public DateTime? PaymentDate { get; set; }
//        public Guid VehicleId { get; set; }
//        public Guid OfficerId { get; set; }
//        public string? EvidenceImageUrl { get; set; }
//        public DateTime CreatedAt { get; set; }

//        // Navigation Properties
//        public VehicleDto? Vehicle { get; set; }
//        public UserDto? Officer { get; set; }
//    }

//    public class CreateViolationDto
//    {
//        [Required]
//        [MaxLength(100)]
//        public string Type { get; set; } = string.Empty;

//        [MaxLength(500)]
//        public string? Description { get; set; }

//        [Required]
//        [MaxLength(200)]
//        public string Location { get; set; } = string.Empty;

//        [Required]
//        public DateTime Timestamp { get; set; }

//        [Required]
//        [Range(0.01, double.MaxValue, ErrorMessage = "Fine amount must be greater than 0")]
//        public decimal FineAmount { get; set; }

//        [Required]
//        public Guid VehicleId { get; set; }

//        [Required]
//        public Guid OfficerId { get; set; }

//        [MaxLength(500)]
//        public string? EvidenceImageUrl { get; set; }
//    }

//    public class UpdateViolationStatusDto
//    {
//        [Required]
//        public string Status { get; set; } = string.Empty; // Pending, Paid, Cancelled
//    }
//}

using System;
using System.ComponentModel.DataAnnotations;

namespace TrafficViolationsAPI.DTOs
{
    public class ViolationDto
    {
        public string Violation_ID { get; set; } = Guid.NewGuid().ToString();
        public string? Violation_Note { get; set; }
        public string? Violation_Location { get; set; }
        public string Plate_Number { get; set; } = string.Empty;
        public string Plate_Type { get; set; } = string.Empty;
        public string Dividing { get; set; } = string.Empty;
        public Guid Violation_Type_ID { get; set; } 
        public Guid Created_By_User_ID { get; set; }
        public DateTime Created_At { get; set; }
        public string? Notes { get; set; }
        public string? ImagePath { get; set; }
        public bool IsSynced { get; set; }

        // Navigation Properties (اختياري، حسب الحاجة في الـ DTO)
        public ViolationTypeDto? ViolationType { get; set; }
        public UserDto? CreatedByUser { get; set; }
    }

    public class CreateViolationDto
    {
        [Required]
       // [MaxLength(20)]
        public string Violation_ID { get; set; } = Guid.NewGuid().ToString();

        public string? Violation_Note { get; set; }

        public string? Violation_Location { get; set; }

        [Required]
        [MaxLength(20)]
        public string Plate_Number { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Plate_Type { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Dividing { get; set; } = string.Empty;

        [Required]
       
       
        public Guid Violation_Type_ID { get; set; }  // بدل string استخدم Guid


        [Required]
      
        public Guid Created_By_User_ID { get; set; } 

        [Required]
        public DateTime Created_At { get; set; }

        public string? Notes { get; set; }
        public string? ImagePath { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UpdateViolationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Paid, Cancelled
    }
}


