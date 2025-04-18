
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GymManagement.Models;

namespace GymManagement.Data
{
    public static class SeedSessions
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, bool clearBeforeSeed = false)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            await context.Database.EnsureCreatedAsync();

            if (clearBeforeSeed)
            {
                context.Bookings.RemoveRange(context.Bookings);
                context.Sessions.RemoveRange(context.Sessions);
                context.GymClasses.RemoveRange(context.GymClasses);
                await context.SaveChangesAsync();
                Console.WriteLine("🧹 Cleared sessions, gym classes and bookings.");
            }

            string[] roles = { "Admin", "Trainer", "Receptionist", "Customer" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            var branches = context.GymBranches.OrderBy(b => b.BranchId).ToList();
            if (branches.Count < 5)
            {
                Console.WriteLine("❌ Please ensure you've added 5 GymBranch entries in OnModelCreating.");
                return;
            }

            var trainerMap = context.Users.OfType<Trainer>().ToDictionary(t => t.Name, t => t.Id);
            var receptionistMap = context.Users.OfType<Receptionist>().ToDictionary(r => r.Name, r => r.Id);
            var customerMap = context.Users.OfType<Customer>().ToDictionary(c => c.Name, c => c.Id);

            if (trainerMap.Count == 0 || receptionistMap.Count == 0 || customerMap.Count == 0)
            {
                Console.WriteLine("❌ Trainer, Receptionist, or Customer not found. Please seed users first.");
                return;
            }

            // Insert GymClasses
            {
                var gymClasses = new List<GymClass>();
                var random = new Random();
                var trainerIds = trainerMap.Values.Take(10).ToList();

                string[] classNames = {
                    "Gentle Flow Yoga",
                    "Core Pilates",
                    "Full Body Strength",
                    "Cardio Burn",
                    "HIIT Power Circuit",
                    "Zumba Dance Party",
                    "Spin & Sweat",
                    "Upper Body Pump",
                    "Power Vinyasa Yoga",
                    "Endurance Challenge"
                };

                string[] descriptions = {
                    "A calming yoga class that enhances flexibility and relaxation.",
                    "Focuses on core strength and posture improvement.",
                    "Targets the entire body to build balanced strength.",
                    "A fast-paced session designed to boost heart rate and burn fat.",
                    "High-intensity interval training to challenge your limits.",
                    "Dance your way to fitness with fun and energetic moves.",
                    "Indoor cycling with rhythm and resistance for endurance.",
                    "Sculpt your upper body with focused strength exercises.",
                    "Dynamic flow yoga that builds heat and power.",
                    "Push your limits with cardio-driven endurance training."
                };

                for (int i = 0; i < classNames.Length; i++)
                {
                    gymClasses.Add(new GymClass
                    {
                        ClassName = classNames[i],
                        AvailableTime = DateTime.Today.AddDays(i),
                        Duration = random.Next(30, 120),
                        Description = descriptions[i],
                        TrainerId = trainerIds[i % trainerIds.Count]
                    });
                }

                context.GymClasses.AddRange(gymClasses);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Inserted 10 updated gym classes.");
            }

            var gymClassIds = context.GymClasses.Select(c => c.GymClassId).ToList();
            var roomIds = context.Rooms.Select(r => r.RoomId).ToList();
            var sessionCategories = Enum.GetValues<SessionCategory>();
            var rand = new Random();

            var sessionNames = new[] {
                "Power Yoga", "Core Crusher", "Full Body Burn", "Spin & Sweat",
                "Strength Surge", "Cardio Blaze", "Mobility Flow", "Endurance Engine",
                "Flex Fusion", "Zen Stretch", "HIIT Riot", "Bootcamp Blast",
                "Core Revival", "Pulse Ride", "Iron Circuit", "Balance Burn"
            };

            int sessionDays = 10;
            int sessionsPerDay = 6;
            var sessions = new List<Session>();

            for (int dayOffset = 0; dayOffset < sessionDays; dayOffset++)
            {
                var date = DateTime.Today.AddDays(dayOffset);
                var shuffledCategories = sessionCategories.OrderBy(_ => rand.Next()).ToArray();

                for (int j = 0; j < sessionsPerDay; j++)
                {
                    var index = (dayOffset * sessionsPerDay + j);

                    var trainerName = trainerMap.Keys.ElementAt(index % trainerMap.Count);
                    var receptionistName = receptionistMap.Keys.ElementAt(index % receptionistMap.Count);
                    var sessionDate = date.AddHours(9 + (j % 4) * 2);
                    var roomId = roomIds[index % roomIds.Count];

                    bool exists = context.Sessions.Any(s =>
                        s.SessionDateTime == sessionDate && s.RoomId == roomId);

                    if (!exists)
                    {
                        var session = new Session
                        {
                            SessionName = sessionNames[index % sessionNames.Length],
                            SessionDateTime = sessionDate,
                            Capacity = rand.Next(10, 25),
                            Category = shuffledCategories[j % shuffledCategories.Length],
                            GymClassId = gymClassIds[index % gymClassIds.Count],
                            RoomId = roomId,
                            TrainerId = trainerMap[trainerName],
                            ReceptionistId = receptionistMap[receptionistName]
                        };

                        context.Sessions.Add(session);
                        sessions.Add(session);
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {sessions.Count} sessions.");

            var customers = customerMap.Values.ToList();
            var receptionists = receptionistMap.Values.ToList();
            var sessionList = context.Sessions.Include(s => s.Bookings).ToList();
            int bookingCount = 0;

            foreach (var session in sessionList)
            {
                int capacity = session.Capacity;
                var selectedCustomers = customers.OrderBy(_ => rand.Next()).Take(rand.Next(capacity - 3, capacity + 1)).ToList();

                foreach (var customerId in selectedCustomers)
                {
                    bool alreadyBooked = session.Bookings.Any(b => b.CustomerId == customerId);
                    if (alreadyBooked) continue;

                    var status = (BookingStatus)rand.Next(0, 4);
                    var booking = new Booking
                    {
                        BookingDate = session.SessionDateTime.AddDays(-rand.Next(1, 5)),
                        Status = status,
                        CustomerId = customerId,
                        SessionId = session.SessionId,
                        UserId = customerId,
                        ReceptionistId = receptionists[rand.Next(receptionists.Count)],
                        CheckInTime = status == BookingStatus.CheckedIn
                            ? session.SessionDateTime.AddMinutes(-rand.Next(5, 15))
                            : null
                    };

                    context.Bookings.Add(booking);
                    bookingCount++;
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {bookingCount} bookings.");
        }
    }
}
