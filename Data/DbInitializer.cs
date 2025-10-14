using EvergreenRanch.Models;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext db)
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
                    new Module { CourseId = stableCourse.Id, Title = "Stable Hygiene", Content = "Cleaning procedures and waste management." },
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
        }
    }
}
