using System;
using System.ComponentModel.DataAnnotations;

namespace TrafficViolationsAPI.DTOs
{
    public class ViolationTypeDto
    {
        public Guid Violation_Type_ID { get; set; }
        public string Violation_Description { get; set; }
        public decimal DefaultFineAmount { get; set; }
    }

    public class CreateViolationTypeDto
    {
        [Required]
        [MaxLength(20)]
        public string Violation_Type_ID { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Violation_Description { get; set; } = string.Empty;
        [Required]
        public decimal DefaultFineAmount { get; set; }
    }
}

//using System;
//using System.ComponentModel.DataAnnotations;

//namespace TrafficViolationsAPI.DTOs
//{
//    public class ViolationTypeDto
//    {
//        public Guid Violation_Type_ID { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string? Violation_Description { get; set; }
//        public decimal DefaultFineAmount { get; set; }
//        public bool IsActive { get; set; }
//        public DateTime CreatedAt { get; set; }
//        public DateTime UpdatedAt { get; set; }
//    }

//    public class CreateViolationTypeDto
//    {
//        [Required]
//        public string Name { get; set; } = string.Empty;

//        [MaxLength(500)]
//        public string? Description { get; set; }

//        [Required]
//        public decimal DefaultFineAmount { get; set; }
//    }
//}



