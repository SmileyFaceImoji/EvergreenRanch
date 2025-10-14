using EvergreenRanch.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            // ✅ Ensure database is ready
            await db.Database.EnsureCreatedAsync();

            // ✅ Optional: force reseed if missing any courses
            if (!await db.Courses.AnyAsync())
            {
                var stableCourse = new Course
                {
                    Title = "Stable Management Basics",
                    Description = "Daily care routines, feeding, and stable hygiene for healthy horses."
                };
                var ridingCourse = new Course
                {
                    Title = "Advanced Riding Techniques",
                    Description = "Balance, control, and safety for confident horse riding."
                };

                await db.Courses.AddRangeAsync(stableCourse, ridingCourse);
                await db.SaveChangesAsync();

                var modules = new List<Module>
                {
                    new Module { CourseId = stableCourse.Id, Title = "Feeding Routines", Content = "Proper feeding schedules and horse nutrition." },
                    new Module { CourseId = stableCourse.Id, Title = "Stable Hygiene", Content = "If just one animal on a horse farm falls ill, the entire herd can quickly become infected. Anyone who has ever experienced something like this, as the owner of a horse farm knows how important it is to clean and disinfect the entire facility, including the horse stable.\r\n\r\nAlthough these tasks are often performed by horse owners or staff, farm owners should know what needs to be done. After all, they are responsible for ensuring that the farm is clean and that the horses live under healthy conditions. An overview of who carries out which tasks and when is part of this.\r\n\r\nBecause good hygiene in the horse stable promotes animal health and the well-being of the horses. The cleaner the horse stable, paddock, tack room, and all other areas of the yard are, the less suitable they are as a habitat for viruses, bacteria and parasites. The result is that pathogens spread less quickly and the animals get sick less often." },
                    new Module { CourseId = stableCourse.Id, Title = "Health Checks", Content = "Recognizing common illnesses early." },

                    new Module { CourseId = ridingCourse.Id, Title = "Posture and Balance", Content = "Learn to maintain a correct riding position." },
                    new Module { CourseId = ridingCourse.Id, Title = "Horse Control", Content = "Techniques for safe mounting and controlling your horse." },
                    new Module { CourseId = ridingCourse.Id, Title = "Advanced Maneuvers", Content = "Practicing complex turns and stops." }
                };
                await db.Modules.AddRangeAsync(modules);
                await db.SaveChangesAsync();

                var testQuestions = new List<TestQuestion>
                {
                    new TestQuestion
                    {
                        CourseId = stableCourse.Id,
                        QuestionText = "How often should you clean the stable?",
                        Choices = new[] { "Once a month", "Every day", "Once a week", "Only when dirty" },
                        CorrectAnswerIndex = 1
                    },
                    new TestQuestion
                    {
                        CourseId = stableCourse.Id,
                        QuestionText = "A healthy horse should have:",
                        Choices = new[] { "Dull coat", "Bright eyes and shiny coat", "Cold legs", "Dry nose" },
                        CorrectAnswerIndex = 1
                    },
                    new TestQuestion
                    {
                        CourseId = ridingCourse.Id,
                        QuestionText = "Good riding posture includes:",
                        Choices = new[] { "Looking down", "Hunched shoulders", "Relaxed back and straight posture", "Gripping reins tightly" },
                        CorrectAnswerIndex = 2
                    },
                    new TestQuestion
                    {
                        CourseId = ridingCourse.Id,
                        QuestionText = "Advanced maneuvers include:",
                        Choices = new[] { "Walking in circles", "Horse side-stepping and stops", "Feeding horse treats", "Changing saddle often" },
                        CorrectAnswerIndex = 1
                    }
                };
                await db.TestQuestions.AddRangeAsync(testQuestions);
                await db.SaveChangesAsync();
            }

            // ✅ Seed default shifts for all existing workers
            await SeedDefaultShiftsAsync(db, userManager);
        }

        private static async Task SeedDefaultShiftsAsync(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            // Check if shifts already exist
            if (await db.Shifts.AnyAsync())
            {
                return; // Shifts already seeded
            }

            // Get all users in Worker role
            var workers = await userManager.GetUsersInRoleAsync("Worker");

            if (!workers.Any())
            {
                return; // No workers yet, shifts will be created when users get promoted
            }

            var shifts = new List<Shift>();
            var locations = new[] { "Main Stable", "Riding Arena", "Training Yard", "Pasture Area" };
            var descriptions = new[]
            {
                "Morning feeding and cleanup",
                "Afternoon training session",
                "Evening stable maintenance",
                "Weekend horse care"
            };

            // Create shifts for the next 7 days for each worker
            foreach (var worker in workers)
            {
                for (int i = 0; i < 7; i++)
                {
                    var shiftDate = DateTime.Today.AddDays(i);

                    // Morning shift
                    shifts.Add(new Shift
                    {
                        WorkerId = worker.Id,
                        StartTime = shiftDate.AddHours(8), // 8:00 AM
                        EndTime = shiftDate.AddHours(12),  // 12:00 PM
                        Location = locations[i % locations.Length],
                        Description = descriptions[i % descriptions.Length]
                    });

                    // Afternoon shift (every other day)
                    if (i % 2 == 0)
                    {
                        shifts.Add(new Shift
                        {
                            WorkerId = worker.Id,
                            StartTime = shiftDate.AddHours(13), // 1:00 PM
                            EndTime = shiftDate.AddHours(17),   // 5:00 PM
                            Location = locations[(i + 1) % locations.Length],
                            Description = descriptions[(i + 1) % descriptions.Length]
                        });
                    }
                }
            }

            await db.Shifts.AddRangeAsync(shifts);
            await db.SaveChangesAsync();
        }

        // ✅ Method to create default shifts for a newly promoted worker
        public static async Task CreateDefaultShiftsForNewWorkerAsync(ApplicationDbContext db, string userId)
        {
            var locations = new[] { "Main Stable", "Riding Arena", "Training Yard", "Pasture Area" };
            var descriptions = new[]
            {
                "Morning feeding and cleanup",
                "Afternoon training session",
                "Evening stable maintenance",
                "Weekend horse care"
            };

            var shifts = new List<Shift>();

            // Create shifts for the next 7 days for the new worker
            for (int i = 0; i < 7; i++)
            {
                var shiftDate = DateTime.Today.AddDays(i);

                // Morning shift
                shifts.Add(new Shift
                {
                    WorkerId = userId,
                    StartTime = shiftDate.AddHours(8), // 8:00 AM
                    EndTime = shiftDate.AddHours(12),  // 12:00 PM
                    Location = locations[i % locations.Length],
                    Description = descriptions[i % descriptions.Length]
                });

                // Afternoon shift (every other day)
                if (i % 2 == 0)
                {
                    shifts.Add(new Shift
                    {
                        WorkerId = userId,
                        StartTime = shiftDate.AddHours(13), // 1:00 PM
                        EndTime = shiftDate.AddHours(17),   // 5:00 PM
                        Location = locations[(i + 1) % locations.Length],
                        Description = descriptions[(i + 1) % descriptions.Length]
                    });
                }
            }

            await db.Shifts.AddRangeAsync(shifts);
            await db.SaveChangesAsync();
        }
    }
}