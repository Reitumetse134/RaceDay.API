using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments/{enrolmentId}/results  (Organiser, owner only)
        [Authorize(Roles = "Organiser")]
        [HttpPost("enrolments/{enrolmentId}/results")]
        public async Task<IActionResult> CaptureResult(int enrolmentId, [FromBody] ResultDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var enrolment = await _context.EventEnrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null) return NotFound("No enrolment with that ID.");
            if (enrolment.Event.UserID != userId) return Forbid();

            // Prevent duplicate result entry for the same enrolment
            var existing = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);

            if (existing != null)
                return Conflict("A result already exists for this enrolment.");

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = dto.FinishTime,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                Pace = dto.Pace
            };

            _context.Results.Add(result);
            enrolment.Status = "confirmed";   // automatically confirm on result capture
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventResults), new { eventId = enrolment.EventID }, result);
        }

        // GET: api/events/{eventId}/results  (public)
        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound("No event with that ID.");

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.EventID == eventId)
                .OrderBy(r => r.FinishTime)
                .Select(r => new
                {
                    r.ResultID,
                    ParticipantName = r.Enrolment.Participant.FirstName + " " + r.Enrolment.Participant.LastName,
                    CategoryName = r.Enrolment.Category.CategoryName,
                    FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss"),
                    r.OverallPosition,
                    r.CategoryPosition,
                    r.Pace
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}