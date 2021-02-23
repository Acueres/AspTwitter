using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AspTwitter.AppData;
using AspTwitter.Models;
using AspTwitter.Authentication;


namespace AspTwitter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly AppDbContext context;

        public EntriesController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/Entries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries()
        {
            return await context.Entries.ToListAsync();
        }

        // GET: api/Entries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Entry>> GetEntry(long id)
        {
            var entry = await context.Entries.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            return entry;
        }

        // PUT: api/Entries/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntry(long id, Entry entry)
        {
            if (!HasPermission(entry.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            context.Entry(entry).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Entries
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Entry>> PostEntry(Entry entry)
        {
            if (!HasPermission(entry.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            context.Entries.Add(entry);
            await context.SaveChangesAsync();

            return Ok(new { Id = entry.Id });
        }

        // DELETE: api/Entries/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(long id)
        {
            Entry entry = await context.Entries.FindAsync(id);

            if (entry is null)
            {
                return NotFound();
            }

            if (!HasPermission(entry.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            context.Entries.Remove(entry);
            await context.SaveChangesAsync();

            return Ok();
        }

        private bool HasPermission(long id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }

        private bool EntryExists(long id)
        {
            return context.Entries.Any(e => e.Id == id);
        }
    }
}
