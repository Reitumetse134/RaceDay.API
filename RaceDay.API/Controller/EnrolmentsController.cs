using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/categories/{categoryId}/enrolments  (Participant only)
        [Authorize(Roles = "Participant")]
        [HttpPost("categories/{categoryId}/enrolments")]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Find the Participant record linked to this User
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return NotFound("Participant profile not found.");

            // Find the category & its event
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null) return NotFound("No category with that ID.");

            // Prevent duplicate enrolment in the same category
            var alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(e => e.ParticipantID == participant.ParticipantID
                            && e.CategoryID == categoryId);

            if (alreadyEnrolled)
                return Conflict("You are already enrolled in this category.");

            var enrolment = new EventEnrolment
            {
                ParticipantID = participant.ParticipantID,
                EventID = category.EventID,
                CategoryID = categoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "pending",
                PaymentStatus = "pending"
            };

            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            // Assign a race number (simple example)
            var raceNumber = $"RD{enrolment.EnrolmentID:D5}";

            return CreatedAtAction(nameof(GetMyEnrolments), null, new
            {
                enrolment.EnrolmentID,
                enrolment.CategoryID,
                enrolment.EventID,
                enrolment.Status,
                enrolment.PaymentStatus,
                RaceNumber = raceNumber
            });
        }

        // GET: api/enrolments/me  (Participant only)
        [Authorize(Roles = "Participant")]
        [HttpGet("enrolments/me")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return NotFound("Participant profile not found.");

            var enrolments = await _context.EventEnrolments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Select(e => new
                {
                    e.EnrolmentID,
                    EventName = e.Event.EventName,
                    EventDate = e.Event.EventDate,
                    CategoryName = e.Category.CategoryName,
                    e.Status,
                    e.PaymentStatus,
                    HasResult = e.Result != null
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/{eventId}/enrolments  (Organiser, owner only)
        [Authorize(Roles = "Organiser")]
        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(eventId);

            if (ev == null) return NotFound("No event with that ID.");
            if (ev.UserID != userId) return Forbid();

            var enrolments = await _context.EventEnrolments
                .Where(e => e.EventID == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentID,
                    ParticipantName = e.Participant.FirstName + " " + e.Participant.LastName,
                    e.Participant.Email,
                    CategoryName = e.Category.CategoryName,
                    e.Status,
                    e.PaymentStatus,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/enrolments/{id}  (Participant, owner only)
        [Authorize(Roles = "Participant")]
        [HttpDelete("enrolments/{id}")]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return NotFound("Participant profile not found.");

            var enrolment = await _context.EventEnrolments.FindAsync(id);
            if (enrolment == null) return NotFound("No enrolment with that ID.");
            if (enrolment.ParticipantID != participant.ParticipantID) return Forbid();

            _context.EventEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}