using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models.Common
{
    public class Payment
    {
        public int Id { get; set; }

        // Relationship to worker
        public string WorkerId { get; set; }

        public double TotalHours { get; set; }
        public double HourlyRate { get; set; }
        public double TotalPay { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public bool IsPaid { get; set; } = false;

        [ForeignKey("WorkerId")]
        public IdentityUser Worker { get; set; }
    }
}
