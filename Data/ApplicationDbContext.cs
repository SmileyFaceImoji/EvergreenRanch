using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Reflection;
using System.Text.Json;

namespace EvergreenRanch.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> Items { get; set; }
        public DbSet<ReturnRequests> ReturnRequests { get; set; }
        public DbSet<ReturnImage> ReturnImages { get; set; }
        public DbSet<HealthCheck> HealthChecks { get; set; }
        public DbSet<OrderFeedback> OrderFeedbacks { get; set; }
        //Courses stuff
        public DbSet<WorkerApplication> WorkerApplications { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Models.Module> Modules { get; set; }
        public DbSet<UserModuleProgress> UserModuleProgresses { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<WorkerTask> WorkerTasks { get; set; }
        public DbSet<WorkerLeave> WorkerLeaves { get; set; }
        public DbSet<WorkerLeaveBalance> WorkerLeaveBalances { get; set; }
        public DbSet<ShiftAttendance> ShiftAttendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<TrainingRegistration> TrainingRegistrations { get; set; }

        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftChangeRequest> ShiftChangeRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TestQuestion>()
                .Property(t => t.ChoicesJson)
                .HasColumnName("ChoicesJson");

            // seed a sample course + modules + questions (safe for dev/demo)
            builder.Entity<Course>().HasData(new Course { Id = 1, Title = "Ranch Worker Basics", Description = "Intro course for ranch workers." });
            builder.Entity<Models.Module>().HasData(
                new Models.Module { Id = 1, CourseId = 1, Title = "Horse Care 101", Content = "Grooming, basics", SortOrder = 1 },
                new Models.Module { Id = 2, CourseId = 1, Title = "Stable Safety", Content = "Safety protocols", SortOrder = 2 }
            );
            builder.Entity<TestQuestion>().HasData(
                new TestQuestion
                {
                    Id = 1,
                    CourseId = 1,
                    QuestionText = "What is the minimum PPE in the stable?",
                    ChoicesJson = JsonSerializer.Serialize(new[] { "Hat", "Gloves", "Helmet", "Flip-flops" }),
                    CorrectAnswerIndex = 1
                },
                new TestQuestion
                {
                    Id = 2,
                    CourseId = 1,
                    QuestionText = "How often should you clean stalls?",
                    ChoicesJson = JsonSerializer.Serialize(new[] { "Daily", "Weekly", "Monthly", "Yearly" }),
                    CorrectAnswerIndex = 0
                }
            );
        }
    }
}
