using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymManagement.Models;

namespace GymManagement.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<GymBranch> GymBranches { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Trainer> Trainers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GymBranch>().HasKey(g => g.BranchId);

            // 设置继承关系（TPH）
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<User>("User")
                .HasValue<Trainer>("Trainer")
                .HasValue<Customer>("Customer")
                .HasValue<Receptionist>("Receptionist");

            // 设置 GymBranch 与 Trainer / Receptionist 的关系
            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.GymBranch)
                .WithMany(b => b.Trainers)
                .HasForeignKey(t => t.BranchId);

            modelBuilder.Entity<Receptionist>()
                .HasOne(r => r.GymBranch)
                .WithMany(b => b.Receptionists)
                .HasForeignKey(r => r.BranchId);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.GymBranch)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.BranchId);

            // 示例数据
            modelBuilder.Entity<GymBranch>().HasData(
                new GymBranch { BranchId = 1, BranchName = "Downtown Gym", Address = "101 Main St", ContactNumber = "555-1001" },
                new GymBranch { BranchId = 2, BranchName = "Uptown Gym", Address = "202 High St", ContactNumber = "555-2002" },
                new GymBranch { BranchId = 3, BranchName = "Eastside Gym", Address = "303 East Ave", ContactNumber = "555-3003" },
                new GymBranch { BranchId = 4, BranchName = "Westside Gym", Address = "404 West Blvd", ContactNumber = "555-4004" },
                new GymBranch { BranchId = 5, BranchName = "Central Gym", Address = "505 Central Rd", ContactNumber = "555-5005" }
);

            modelBuilder.Entity<Room>().HasData(
                // Rooms for Branch 1
                new Room { RoomId = 1, RoomName = "Recovery Room", Capacity = 44, IsAvailable = true, BranchId = 1 },
                new Room { RoomId = 2, RoomName = "Mobility Area", Capacity = 21, IsAvailable = true, BranchId = 1 },
                new Room { RoomId = 3, RoomName = "Endurance Zone", Capacity = 35, IsAvailable = true, BranchId = 1 },
                new Room { RoomId = 4, RoomName = "Functional Room", Capacity = 32, IsAvailable = true, BranchId = 1 },
                new Room { RoomId = 5, RoomName = "Dance Studio", Capacity = 47, IsAvailable = true, BranchId = 1 },

                // Rooms for Branch 2
                new Room { RoomId = 6, RoomName = "Endurance Zone", Capacity = 40, IsAvailable = true, BranchId = 2 },
                new Room { RoomId = 7, RoomName = "Spin Studio", Capacity = 30, IsAvailable = true, BranchId = 2 },
                new Room { RoomId = 8, RoomName = "Cardio Room", Capacity = 40, IsAvailable = true, BranchId = 2 },
                new Room { RoomId = 9, RoomName = "Dance Studio", Capacity = 49, IsAvailable = true, BranchId = 2 },
                new Room { RoomId = 10, RoomName = "Strength Studio", Capacity = 33, IsAvailable = true, BranchId = 2 },

                // Rooms for Branch 3
                new Room { RoomId = 11, RoomName = "Stretch Zone", Capacity = 48, IsAvailable = true, BranchId = 3 },
                new Room { RoomId = 12, RoomName = "Crossfit Zone", Capacity = 20, IsAvailable = true, BranchId = 3 },
                new Room { RoomId = 13, RoomName = "Cardio Room", Capacity = 36, IsAvailable = true, BranchId = 3 },
                new Room { RoomId = 14, RoomName = "Strength Studio", Capacity = 32, IsAvailable = true, BranchId = 3 },
                new Room { RoomId = 15, RoomName = "Functional Room", Capacity = 26, IsAvailable = true, BranchId = 3 },

                // Rooms for Branch 4
                new Room { RoomId = 16, RoomName = "HIIT Area", Capacity = 49, IsAvailable = true, BranchId = 4 },
                new Room { RoomId = 17, RoomName = "Dance Studio", Capacity = 29, IsAvailable = true, BranchId = 4 },
                new Room { RoomId = 18, RoomName = "Mobility Area", Capacity = 39, IsAvailable = true, BranchId = 4 },
                new Room { RoomId = 19, RoomName = "Yoga Room", Capacity = 45, IsAvailable = true, BranchId = 4 },
                new Room { RoomId = 20, RoomName = "Stretch Zone", Capacity = 50, IsAvailable = true, BranchId = 4 },

                // Rooms for Branch 5
                new Room { RoomId = 21, RoomName = "Cardio Room", Capacity = 41, IsAvailable = true, BranchId = 5 },
                new Room { RoomId = 22, RoomName = "Weight Room", Capacity = 24, IsAvailable = true, BranchId = 5 },
                new Room { RoomId = 23, RoomName = "Mobility Area", Capacity = 29, IsAvailable = true, BranchId = 5 },
                new Room { RoomId = 24, RoomName = "Recovery Room", Capacity = 44, IsAvailable = true, BranchId = 5 },
                new Room { RoomId = 25, RoomName = "Endurance Zone", Capacity = 42, IsAvailable = true, BranchId = 5 }
            );


        }
    }
}
