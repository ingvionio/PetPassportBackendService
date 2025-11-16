using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/events/next?petId=1
        [HttpGet("next")]
        public async Task<ActionResult<IEnumerable<PetEvent>>> GetNextEvents(int petId)
        {
            var now = DateTime.UtcNow;

            var events = await _context.Events
                .Where(e => e.PetId == petId)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            return Ok(events);
        }
    }
}
