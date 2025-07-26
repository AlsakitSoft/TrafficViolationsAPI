using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficViolationsAPI.Models
{
    //public class Violation
    //{
    //    [Key]
    //    public Guid Id { get; set; } = Guid.NewGuid();

    //    [Required]
    //    [MaxLength(100)]
    //    public string Type { get; set; } = string.Empty;

    //    [MaxLength(500)]
    //    public string? Description { get; set; }

    //    [Required]
    //    [MaxLength(200)]
    //    public string Location { get; set; } = string.Empty;

    //    [Required]
    //    public DateTime Timestamp { get; set; }

    //    [Required]
    //    [Column(TypeName = "decimal(10,2)")]
    //    public decimal FineAmount { get; set; }

    //    [Required]
    //    [MaxLength(20)]
    //    public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

    //    public DateTime? PaymentDate { get; set; }

    //    [Required]
    //    public Guid VehicleId { get; set; }

    //    [Required]
    //    public Guid OfficerId { get; set; }

    //    [MaxLength(500)]
    //    public string? EvidenceImageUrl { get; set; }

    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    //    // Computed Property
    //    [NotMapped]
    //    public bool IsPaid => Status == "Paid";

    //    // Navigation Properties
    //    [ForeignKey("VehicleId")]
    //    public virtual Vehicle Vehicle { get; set; } = null!;

    //    [ForeignKey("OfficerId")]
    //    public virtual User Officer { get; set; } = null!;
    //}
    
        public class Violation
        {
            [Key]
           
            public string Violation_ID { get; set; } = Guid.NewGuid().ToString(); // اقتراح: توليد ID تلقائي إذا لم يكن لديك ID يدوي

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
        public Guid Violation_Type_ID { get; set; }

        [Required]
            public Guid Created_By_User_ID { get; set; }

            [Required]
            public DateTime Created_At { get; set; } = DateTime.UtcNow;

            public string Status { get; set; } = "Pending";

            // Navigation Properties
            [ForeignKey("Violation_Type_ID")]
            public virtual ViolationType? ViolationType { get; set; }

            [ForeignKey("Created_By_User_ID")]
            public virtual User CreatedByUser { get; set; } = null!;
        }
    }
