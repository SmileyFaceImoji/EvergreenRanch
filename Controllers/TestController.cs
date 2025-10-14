using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public TestController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // GET: /Test/Start/1
        public async Task<IActionResult> Start(int courseId)
        {
            var userId = _um.GetUserId(User)!;

            // check all modules completed
            var modules = await _db.Modules.Where(m => m.CourseId == courseId).ToListAsync();
            var completedCount = await _db.UserModuleProgresses.CountAsync(p => p.UserId == userId && p.IsCompleted && modules.Select(m => m.Id).Contains(p.ModuleId));
            if (modules.Count == 0 || completedCount < modules.Count)
            {
                TempData["Message"] = "Complete all modules first.";
                return RedirectToAction("Modules", "Training", new { courseId });
            }

            var questions = await _db.TestQuestions.Where(t => t.CourseId == courseId).ToListAsync();
            return View(new TestStartViewModel { CourseId = courseId, Questions = questions });
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int courseId, Dictionary<int, int> answers)
        {
            var userId = _um.GetUserId(User)!;
            var questions = await _db.TestQuestions.Where(t => t.CourseId == courseId).ToListAsync();
            int correct = 0;

            foreach (var q in questions)
            {
                if (answers != null && answers.TryGetValue(q.Id, out var sel) && sel == q.CorrectAnswerIndex)
                    correct++;
            }

            int scorePercent = (int)Math.Round(questions.Count == 0 ? 0 : (correct * 100.0 / questions.Count));
            bool passed = scorePercent >= 70;

            var attempt = new TestAttempt
            {
                UserId = userId,
                CourseId = courseId,
                Score = scorePercent,
                Passed = passed,
                AttemptedOn = DateTime.UtcNow
            };
            _db.TestAttempts.Add(attempt);

            if (passed)
            {
                // ✅ Issue certificate if not already issued
                var exists = await _db.Certificates.AnyAsync(c => c.UserId == userId && c.CourseId == courseId);
                if (!exists)
                {
                    _db.Certificates.Add(new Certificate
                    {
                        UserId = userId,
                        CourseId = courseId,
                        IssuedOn = DateTime.UtcNow,
                        CertificateGuid = Guid.NewGuid().ToString()
                    });
                }

                // ✅ Promote to Worker role if not already assigned
                var user = await _um.FindByIdAsync(userId);
                if (user != null && !await _um.IsInRoleAsync(user, "Worker"))
                {
                    await _um.AddToRoleAsync(user, "Worker");

                    // ✅ Create default shifts for the new worker
                    await DbInitializer.CreateDefaultShiftsForNewWorkerAsync(_db, userId);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Result", new { attemptId = attempt.Id });
        }
        public async Task<IActionResult> Result(int attemptId)
        {
            var attempt = await _db.TestAttempts.FindAsync(attemptId);
            if (attempt == null) return NotFound();
            return View(attempt);
        }
    }

    public class TestStartViewModel
    {
        public int CourseId { get; set; }
        public List<TestQuestion> Questions { get; set; } = new();
    }
}
