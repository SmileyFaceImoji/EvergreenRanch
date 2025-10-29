using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class ShiftAttendance
    {
        [Key]
        public int Id { get; set; }

        public int ShiftId { get; set; }

        [ForeignKey(nameof(ShiftId))]
        public virtual Shift Shift { get; set; } = default!;

        public double hourlyRate { get; set; }=120.00;

        public string WorkerId { get; set; } = default!;

        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        [NotMapped]
        public double TotalHours => ClockOutTime.HasValue
            ? (ClockOutTime.Value - ClockInTime).TotalHours
            : 0;

        [NotMapped]
        public bool IsLate => ClockInTime > Shift.StartTime.AddMinutes(5);

        [NotMapped]
        public bool LeftEarly => ClockOutTime.HasValue && ClockOutTime < Shift.EndTime.AddMinutes(-5);
    }
}
