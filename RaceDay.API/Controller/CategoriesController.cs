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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/categories  (public)
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound("No event with that ID.");

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/{eventId}/categories  (Organiser, owner only)
        [Authorize(Roles = "Organiser")]
        [HttpPost("events/{eventId}/categories")]
        public async Task<IActionResult> AddCategory(int eventId, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(eventId);

            if (ev == null) return NotFound("No event with that ID.");
            if (ev.UserID != userId) return Forbid();

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName,
                Gender = dto.Gender,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriesForEvent), new { eventId }, category);
        }

        // PUT: api/categories/{id}  (Organiser, owner only)
        [Authorize(Roles = "Organiser")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound("No category with that ID.");
            if (category.Event.UserID != userId) return Forbid();

            category.CategoryName = dto.CategoryName;
            category.Gender = dto.Gender;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/categories/{id}  (Organiser, owner only)
        [Authorize(Roles = "Organiser")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound("No category with that ID.");
            if (category.Event.UserID != userId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}