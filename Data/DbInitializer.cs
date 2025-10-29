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
                    new Module { CourseId = stableCourse.Id, Title = "Feeding Routines", Content =@" Feeding is one of the most critical responsibilities in stable management. Horses are creatures of habit; they thrive on routine and consistency. This module will walk you through the science and daily practice of feeding.

### Topics Covered:
- **Understanding Horse Digestion:**
  Horses are grazing animals with sensitive digestive systems.Their stomachs are small relative to their body size, meaning they should eat small amounts frequently rather than large meals.


- **Types of Feed:**
  1. * *Roughage:**Grass, hay, and haylage — forms the majority of a horse's diet(at least 60 %).
  2. * *Concentrates:**Grains and pelleted feeds for additional energy.
  3. * *Supplements:**Vitamins, minerals, and electrolytes for specific needs.


-**Feeding Schedules:**
  -Feed at the * *same times daily * *, ideally in 3 - 4 smaller meals.
  - Ensure * *fresh water * * is always available.
  -Never feed immediately before or after intense exercise; allow rest periods.


- **Feeding Safety:**
  -Check for **mouldy hay * *or spoiled feed.
  - Avoid sudden diet changes — they can trigger colic.
  - Use separate buckets for each horse to avoid conflict.

### Key Insight:
A horse's health mirrors its diet.A shiny coat, steady energy, and calm temperament often mean proper feeding."
                     },
                    new Module { CourseId = stableCourse.Id, Title = "Stable Hygiene", Content = @"
Stable hygiene directly influences horse welfare and disease prevention. One overlooked stall can become a breeding ground for bacteria or parasites that spread across the facility.

### Cleaning Protocol:
- **Daily Tasks:**
  - Remove manure and soiled bedding.
  - Check and refill water buckets.
  - Sweep aisles and remove dust.
- **Weekly Tasks:**
  - Replace bedding entirely.
  - Disinfect feed bins and buckets.
  - Scrub down stable walls and partitions.
- **Monthly Tasks:**
  - Power wash drains and floor areas.
  - Treat the stable with safe disinfectants.
  - Rotate horses between stalls to rest flooring.

### Biosecurity Measures:
- Quarantine new or sick horses immediately.
- Wash hands and tools between horses.
- Use separate grooming kits for each animal.
- Limit visitor access to stables.
- Keep veterinary and cleaning records updated.

### Environmental Control:
- Maintain **proper ventilation** to reduce ammonia buildup.
- Control **rodents and insects** to prevent contamination.
- Ensure **good lighting** for inspections.

A clean stable not only prevents illness but also reduces stress in horses, improving their behavior and performance."
                     },
                    new Module { CourseId = stableCourse.Id, Title = "Health Checks", Content =@"
Early detection of health problems can save a horse's life. Horses hide pain instinctively, so recognizing subtle signs is key.

### Vital Signs (Know These Numbers):
- **Temperature:** 37.5°C – 38.5°C
- **Pulse:** 28–44 beats per minute
- **Respiration:** 10–24 breaths per minute
- **Capillary Refill Time:** Under 2 seconds

### Common Warning Signs:
- Loss of appetite or refusal to drink
- Unusual sweating or lethargy
- Swelling in legs or joints
- Changes in manure (dry or watery)
- Head tilting, coughing, or nasal discharge

### Daily Health Routine:
1. Inspect the horse before and after work.
2. Check eyes, nose, and hooves.
3. Feel the legs for heat or swelling.
4. Keep logs for each horse's health and behavior patterns.

### Common Ailments:
- **Colic:** Abdominal pain — requires immediate attention.
- **Laminitis:** Inflammation in the hooves due to diet or stress.
- **Respiratory Infection:** Caused by poor ventilation or dust.
- **Parasites:** Prevent through deworming and clean feeding.

Recognizing illness early can prevent suffering and expensive veterinary bills."
                     },

                    new Module { CourseId = ridingCourse.Id, Title = "Posture and Balance", Content = @"
The foundation of good riding is posture. A rider's balance influences the horse's comfort, movement, and trust.

### Correct Posture Checklist:
- Sit tall, shoulders relaxed but open.
- Heels down, toes slightly pointed out.
- Keep hands soft, elbows close to your body.
- Look forward — never down.

### Core Strength Development:
- Off-horse exercises: planks, leg lifts, and squats.
- Riding exercises: no-stirrup work to enhance stability.
- Breathing techniques: steady breaths keep rhythm and calmness.

### Balance in Motion:
Your horse mirrors your movements. If you lean too far forward or backward, the horse adjusts awkwardly. Learn to move in harmony — flowing with each stride rather than fighting it.

Consistency builds trust. Over time, your horse learns to anticipate your cues through subtle shifts in posture."
                     },
                    new Module { CourseId = ridingCourse.Id, Title = "Horse Control", Content =  @"
Communication between rider and horse relies on subtlety. The less visible your commands, the better trained your horse appears.

### Core Principles:
- **Reins:** Direct pressure should be gentle — never pull. Release when the horse responds.
- **Legs:** Apply steady pressure to move forward or sideways.
- **Voice:** Use consistent tones. Horses respond to calm, low voices.
- **Seat:** Shift your weight slightly to guide direction.

### Exercises for Communication:
1. Practice halts and transitions smoothly.
2. Work on circles and serpentines for flexibility.
3. Ride without reins (under supervision) to improve balance and trust.

A truly skilled rider appears to communicate effortlessly — that's your goal."
                     },
                    new Module { CourseId = ridingCourse.Id, Title = "Advanced Maneuvers", Content =  @"
This module introduces professional techniques for competitive or performance riding.

### Techniques Covered:
- **Half-Pass:** Diagonal movement requiring balance and leg coordination.
- **Flying Lead Change:** Changing leg leads mid-canter.
- **Collection and Extension:** Controlling stride length with rhythm.
- **Rollback Turn:** Quick directional shift used in show jumping.
- **Jumping Basics:** Take-off, landing, and rider positioning.

Each exercise strengthens your control and builds your horse's agility and confidence."
                    }
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
                    },
                     new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What should make up the majority of a horse's diet?", Choices = new[] { "Grains", "Roughage (hay/grass)", "Supplements", "Fruits" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "How often should stalls be cleaned?", Choices = new[] { "Daily", "Every 3 days", "Once a week", "Once a month" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What is the normal pulse rate for a healthy adult horse?", Choices = new[] { "10–15 bpm", "28–44 bpm", "50–60 bpm", "70–90 bpm" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What should you do first during a fire emergency?", Choices = new[] { "Run for help", "Release all horses", "Calmly lead horses to safety one at a time", "Hide in the office" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "Which of these is a key sign of laminitis?", Choices = new[] { "Swollen eyes", "Hoof heat and reluctance to move", "Dry coat", "Loss of appetite" }, CorrectAnswerIndex = 1 },

                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Where should your heels be during proper posture?", Choices = new[] { "Up", "Flat", "Down", "Doesn't matter" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which cue uses the rider's weight to guide the horse?", Choices = new[] { "Voice", "Seat", "Reins", "Legs" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "A flying lead change is performed during which gait?", Choices = new[] { "Walk", "Trot", "Canter", "Gallop" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What is the correct action when passing another rider in the arena?", Choices = new[] { "Pass on the left", "Pass on the right", "Shout to warn them", "Stop and wait" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Why is tack inspection important?", Choices = new[] { "To keep it shiny", "To prevent equipment failure and injury", "To impress others", "To follow tradition" }, CorrectAnswerIndex = 1 },
 // Stable Management
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What is the ideal ratio of roughage to concentrates in a horse's diet?", Choices = new[] { "90:10", "70:30", "60:40", "40:60" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "How many liters of water should a 500kg horse drink daily?", Choices = new[] { "5–10", "10–20", "20–40", "50–70" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What causes colic most often?", Choices = new[] { "Dirty bedding", "Sudden diet change", "Cold weather", "Short grooming" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "When should stalls be disinfected?", Choices = new[] { "Once a month", "Once a week", "After every new horse", "Only during summer" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "Which vital sign shows respiration rate?", Choices = new[] { "Heartbeats per minute", "Breaths per minute", "Steps per minute", "Temperature" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "How do you handle a bleeding wound?", Choices = new[] { "Apply pressure with clean cloth", "Rinse with soap", "Tie rope above wound", "Ignore until vet arrives" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "What is the normal resting temperature of a horse?", Choices = new[] { "35°C", "37–38°C", "39°C", "40°C" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "Which area should be disinfected first during a disease outbreak?", Choices = new[] { "Feed storage", "Aisles", "Quarantine stalls", "Paddocks" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "Which of these is a sign of dehydration?", Choices = new[] { "Shiny coat", "Skin stays up when pinched", "Wet nose", "Energetic behavior" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = stableCourse.Id, QuestionText = "Why is record-keeping important?", Choices = new[] { "To impress inspectors", "To monitor health trends and identify problems early", "For decoration", "For feeding experiments" }, CorrectAnswerIndex = 1 },

                    // Riding Course
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which part of the body should align vertically when seated correctly?", Choices = new[] { "Head, knees, toes", "Ears, shoulders, hips, heels", "Back and tailbone", "Neck and knees" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What is the correct reaction if your horse suddenly spooks?", Choices = new[] { "Pull hard on reins", "Shout loudly", "Stay calm and re-center balance", "Kick the horse forward" }, CorrectAnswerIndex = 2 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which cue primarily signals a speed increase?", Choices = new[] { "Rein pull", "Leg pressure", "Voice command only", "Leaning forward" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "When performing a half-pass, the horse moves:", Choices = new[] { "Forward and sideways", "Backward", "In a circle", "In place" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What is a key principle in communication with the horse?", Choices = new[] { "Force for obedience", "Consistency and softness", "Loud commands", "Fast movements" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which safety check is most critical before every ride?", Choices = new[] { "Helmet fit and girth security", "Boot color", "Arena cleanliness", "Weather report" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "After jumping, what should a rider immediately do?", Choices = new[] { "Stop suddenly", "Maintain steady contact and allow recovery", "Drop reins", "Dismount" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which etiquette rule applies on trails?", Choices = new[] { "Ride fast when alone", "Leave gates as you found them", "Approach others silently", "Ignore livestock" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What helps a horse stay calm during advanced training?", Choices = new[] { "Routine, clear cues, and reward", "Random exercises", "Harsh corrections", "Long breaks" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What is the first step when your tack feels loose during a ride?", Choices = new[] { "Jump off immediately", "Stop and recheck safely", "Ignore it", "Ride faster" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "What does proper breathing do for the rider?", Choices = new[] { "Increases stamina and keeps rhythm", "Makes the horse sleepy", "Cools the saddle", "Improves vision" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "When riding in groups, the safest distance between horses is:", Choices = new[] { "1 meter", "A horse length", "Half a meter", "Touching nose to tail" }, CorrectAnswerIndex = 1 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Which maneuver helps improve lateral flexibility?", Choices = new[] { "Leg yield", "Reverse turn", "Jumping", "Backing up" }, CorrectAnswerIndex = 0 },
                    new TestQuestion { CourseId = ridingCourse.Id, QuestionText = "Why should reins not be held too tightly?", Choices = new[] { "It causes pain and loss of communication", "It looks unprofessional", "It slows down speed", "It's unsafe for jumping" }, CorrectAnswerIndex = 0 }

                };
                await db.TestQuestions.AddRangeAsync(testQuestions);
                await db.SaveChangesAsync();
            }

            // ✅ TEMPORARY: Give every worker 20 leave days for the year so far
            var workers = await userManager.GetUsersInRoleAsync("Worker");

            foreach (var worker in workers)
            {
                var existingBalance = await db.WorkerLeaveBalances
                    .FirstOrDefaultAsync(b => b.UserId == worker.Id);

                if (existingBalance == null)
                {
                    db.WorkerLeaveBalances.Add(new WorkerLeaveBalance
                    {
                        UserId = worker.Id,
                        AvailableDays = 20,
                        LastAccrualDate = DateTime.UtcNow
                    });
                }
                else
                {
                    existingBalance.AvailableDays = 20;
                    existingBalance.LastAccrualDate = DateTime.UtcNow;
                    db.WorkerLeaveBalances.Update(existingBalance);
                }
            }

            await db.SaveChangesAsync();

            // ✅ Give every worker 5 hours of work time by creating shift attendances
            foreach (var worker in workers)
            {
                // Check if worker already has at least 5 hours of attendance
                var totalHours = await db.ShiftAttendances
                    .Where(a => a.WorkerId == worker.Id)
                    .SumAsync(a =>
    a.ClockOutTime.HasValue
        ? (a.ClockOutTime.Value - a.ClockInTime).TotalHours
        : 0
);

                if (totalHours < 5)
                {
                    // Create a shift specifically to reach 5 hours
                    var shift = new Shift
                    {
                        WorkerId = worker.Id,
                        StartTime = DateTime.Today.AddHours(8), // 8:00 AM today
                        EndTime = DateTime.Today.AddHours(13),  // 1:00 PM (5 hours)
                        Location = "Main Stable",
                        Description = "Initial training shift"
                    };

                    await db.Shifts.AddAsync(shift);
                    await db.SaveChangesAsync(); // Save to get shift ID

                    // Create attendance for this shift
                    db.ShiftAttendances.Add(new ShiftAttendance
                    {
                        ShiftId = shift.Id,
                        WorkerId = worker.Id,
                        ClockInTime = shift.StartTime,
                        ClockOutTime = shift.EndTime
                    });
                }
            }

            await db.SaveChangesAsync();

            // ✅ ADD 48-HOUR SHIFT FOR ALL WORKERS
            foreach (var worker in workers)
            {
                // Check if worker already has a 48-hour shift to avoid duplicates
                var existing48HourShift = await db.Shifts
                    .FirstOrDefaultAsync(s => s.WorkerId == worker.Id &&
                                            (s.EndTime - s.StartTime).TotalHours == 48);

                if (existing48HourShift == null)
                {
                    // Create 48-hour shift starting tomorrow at 8:00 AM
                    var startTime = DateTime.Today.AddDays(1).AddHours(8);
                    var endTime = startTime.AddHours(48);

                    var longShift = new Shift
                    {
                        WorkerId = worker.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        Location = "Emergency Coverage - All Areas",
                        Description = "48-hour extended coverage shift for special event"
                    };

                    await db.Shifts.AddAsync(longShift);
                    await db.SaveChangesAsync(); // Save to get shift ID

                    // Create attendance record for the 48-hour shift
                    db.ShiftAttendances.Add(new ShiftAttendance
                    {
                        ShiftId = longShift.Id,
                        WorkerId = worker.Id,
                        ClockInTime = startTime,
                        ClockOutTime = endTime
                    });
                }
            }

            await db.SaveChangesAsync();

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

            // ✅ Ensure each worker has at least 5 hours of attendance logged
            var allShifts = await db.Shifts.ToListAsync();

            foreach (var worker in workers)
            {
                // Check if the worker already has attendance records
                var existingAttendances = await db.ShiftAttendances
                    .Where(a => a.WorkerId == worker.Id)
                    .ToListAsync();

                if (!existingAttendances.Any())
                {
                    // Find up to 2 shifts per worker to total around 5 hours minimum
                    var assignedShifts = allShifts
                        .Where(s => s.WorkerId == worker.Id)
                        .Take(2)
                        .ToList();

                    foreach (var shift in assignedShifts)
                    {
                        db.ShiftAttendances.Add(new ShiftAttendance
                        {
                            ShiftId = shift.Id,
                            WorkerId = worker.Id,
                            ClockInTime = shift.StartTime,
                            ClockOutTime = shift.StartTime.AddHours(2.5) // ~2.5 hours per shift → 5h total
                        });
                    }
                }
            }

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

            // ✅ ADD 48-HOUR SHIFT FOR THE NEW WORKER
            var startTime = DateTime.Today.AddDays(1).AddHours(8);
            var endTime = startTime.AddHours(48);

            var longShift = new Shift
            {
                WorkerId = userId,
                StartTime = startTime,
                EndTime = endTime,
                Location = "Emergency Coverage - All Areas",
                Description = "48-hour extended coverage shift for special event"
            };

            await db.Shifts.AddAsync(longShift);
            await db.SaveChangesAsync(); // Save to get shift ID

            // Create attendance record for the 48-hour shift
            db.ShiftAttendances.Add(new ShiftAttendance
            {
                ShiftId = longShift.Id,
                WorkerId = userId,
                ClockInTime = startTime,
                ClockOutTime = endTime
            });

            await db.SaveChangesAsync();

            // Seed attendances for each shift so payments work
            var allShifts = await db.Shifts.Where(s => s.WorkerId == userId).ToListAsync();

            foreach (var shift in allShifts)
            {
                // Only create attendance if none exists (for regular shifts)
                if (!await db.ShiftAttendances.AnyAsync(a => a.ShiftId == shift.Id && (shift.EndTime - shift.StartTime).TotalHours != 48))
                {
                    db.ShiftAttendances.Add(new ShiftAttendance
                    {
                        ShiftId = shift.Id,
                        WorkerId = userId,
                        ClockInTime = shift.StartTime,
                        ClockOutTime = shift.EndTime
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}