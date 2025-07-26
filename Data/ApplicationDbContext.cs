//using Microsoft.EntityFrameworkCore;
//using TrafficViolationsAPI.Models;

//namespace TrafficViolationsAPI.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
//        {
//        }

//        public DbSet<User> Users { get; set; }
//        public DbSet<Vehicle> Vehicles { get; set; }
//        public DbSet<Violation> Violations { get; set; }
//        public DbSet<ViolationType> ViolationTypes { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // User Configuration
//            modelBuilder.Entity<User>(entity =>
//            {
//                entity.HasIndex(e => e.NationalId).IsUnique();
//                entity.HasIndex(e => e.Email).IsUnique();
//                entity.Property(e => e.UserType).HasDefaultValue("Citizen");
//                entity.Property(e => e.IsActive).HasDefaultValue(true);
//                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
//                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
//            });

//            // Vehicle Configuration
//            modelBuilder.Entity<Vehicle>(entity =>
//            {
//                entity.HasIndex(e => e.PlateNumber).IsUnique();
//                entity.Property(e => e.IsActive).HasDefaultValue(true);
//                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
//                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

//                entity.HasOne(v => v.Owner)
//                      .WithMany(u => u.Vehicles)
//                      .HasForeignKey(v => v.OwnerId)
//                      .OnDelete(DeleteBehavior.Restrict);
//            });

//            // Violation Configuration
//            modelBuilder.Entity<Violation>(entity =>
//            {
//                entity.Property(e => e.Status).HasDefaultValue("Pending");
//                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
//                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

//                entity.HasOne(v => v.Vehicle)
//                      .WithMany(ve => ve.Violations)
//                      .HasForeignKey(v => v.VehicleId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(v => v.Officer)
//                      .WithMany(u => u.ViolationsIssued)
//                      .HasForeignKey(v => v.OfficerId)
//                      .OnDelete(DeleteBehavior.Restrict);
//            });

//            // ViolationType Configuration
//            modelBuilder.Entity<ViolationType>(entity =>
//            {
//                entity.HasIndex(e => e.Name).IsUnique();
//                entity.Property(e => e.IsActive).HasDefaultValue(true);
//                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
//                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
//            });

//            // Seed Data
//            SeedData(modelBuilder);
//        }

//        private void SeedData(ModelBuilder modelBuilder)
//        {
//            // Seed ViolationTypes
//            var violationTypes = new[]
//            {
//                new ViolationType
//                {
//                    Id = Guid.NewGuid(),
//                    Name = "تجاوز السرعة المحددة",
//                    Description = "تجاوز الحد الأقصى للسرعة المسموح بها",
//                    DefaultFineAmount = 300.00m,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                },
//                new ViolationType
//                {
//                    Id = Guid.NewGuid(),
//                    Name = "عدم ربط حزام الأمان",
//                    Description = "عدم استخدام حزام الأمان أثناء القيادة",
//                    DefaultFineAmount = 150.00m,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                },
//                new ViolationType
//                {
//                    Id = Guid.NewGuid(),
//                    Name = "استخدام الهاتف أثناء القيادة",
//                    Description = "استخدام الهاتف المحمول أثناء قيادة المركبة",
//                    DefaultFineAmount = 500.00m,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                },
//                new ViolationType
//                {
//                    Id = Guid.NewGuid(),
//                    Name = "عدم التوقف عند الإشارة الحمراء",
//                    Description = "تجاوز الإشارة الضوئية الحمراء",
//                    DefaultFineAmount = 1000.00m,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                },
//                new ViolationType
//                {
//                    Id = Guid.NewGuid(),
//                    Name = "الوقوف في مكان ممنوع",
//                    Description = "إيقاف المركبة في مكان غير مسموح",
//                    DefaultFineAmount = 200.00m,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                }
//            };

//            modelBuilder.Entity<ViolationType>().HasData(violationTypes);
//        }
//    }
//}


using Microsoft.EntityFrameworkCore;
using TrafficViolationsAPI.Models;


namespace TrafficViolationsAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Violation> Violations { get; set; }
        public DbSet<ViolationType> ViolationTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.NationalId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserType).HasDefaultValue("Citizen");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Vehicle Configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(e => e.PlateNumber).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(v => v.Owner)
                      .WithMany(u => u.Vehicles)
                      .HasForeignKey(v => v.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Violation Configuration
            modelBuilder.Entity<Violation>(entity =>
            {
                entity.HasKey(v => v.Violation_ID); // Set Violation_ID as primary key
                entity.Property(e => e.Created_At).HasDefaultValueSql("GETUTCDATE()");

                // Configure foreign key for ViolationType
                entity.HasOne(v => v.ViolationType)
                      .WithMany()
                      .HasForeignKey(v => v.Violation_Type_ID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure foreign key for CreatedByUser
                entity.HasOne(v => v.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(v => v.Created_By_User_ID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ViolationType Configuration
            modelBuilder.Entity<ViolationType>(entity =>
            {
                entity.HasKey(vt => vt.Violation_Type_ID); // Assuming Violation_Type_ID is the primary key
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed Data
            // SeedData(modelBuilder); // Removed for now, as it might cause issues with new schema
        }

        // private void SeedData(ModelBuilder modelBuilder)
        // {
        //     // Seed ViolationTypes
        //     var violationTypes = new[]
        //     {
        //         new ViolationType
        //         {
        //             Violation_Type_ID = "VT001",
        //             Violation_Description = "تجاوز السرعة المحددة",
        //             CreatedAt = DateTime.UtcNow,
        //             UpdatedAt = DateTime.UtcNow
        //         },
        //         new ViolationType
        //         {
        //             Violation_Type_ID = "VT002",
        //             Violation_Description = "عدم ربط حزام الأمان",
        //             CreatedAt = DateTime.UtcNow,
        //             UpdatedAt = DateTime.UtcNow
        //         }
        //     };

        //     modelBuilder.Entity<ViolationType>().HasData(violationTypes);
        // }
    }
}


