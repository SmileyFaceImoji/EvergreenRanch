using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class CertificateController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _cpf;

        public CertificateController(ApplicationDbContext db, UserManager<IdentityUser> um, IUserClaimsPrincipalFactory<IdentityUser> cpf)
        {
            _db = db;
            _um = um;
            _cpf = cpf;
        }

        // GET: /Certificate/MyCertificates
        public async Task<IActionResult> MyCertificates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var certificates = await _db.Certificates
                .Where(c => c.UserId == userId)
                .Join(_db.Courses,
                    certificate => certificate.CourseId,
                    course => course.Id,
                    (certificate, course) => new CertificateViewModel
                    {
                        Certificate = certificate,
                        UserName = User.Identity.Name ?? "User",
                        CourseTitle = course.Title,
                        CourseDescription = course.Description
                    })
                .OrderByDescending(c => c.Certificate.IssuedOn)
                .ToListAsync();

            return View(certificates);
        }

        // GET: /Certificate/View/abc123
        public async Task<IActionResult> ViewCertificate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var certificate = await _db.Certificates
                .Where(c => c.CertificateGuid == id && c.UserId == userId)
                .Join(_db.Courses,
                    cert => cert.CourseId,
                    course => course.Id,
                    (cert, course) => new CertificateViewModel
                    {
                        Certificate = cert,
                        UserName = User.Identity.Name ?? "User",
                        CourseTitle = course.Title,
                        CourseDescription = course.Description
                    })
                .FirstOrDefaultAsync();

            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        public class CertificateViewModel
        {
            public Certificate Certificate { get; set; } = default!;
            public string UserName { get; set; } = default!;
            public string CourseTitle { get; set; }
            public string? CourseDescription { get; set; }
        }
    }
}