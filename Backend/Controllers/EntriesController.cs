using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntry(long id, Entry entry)
        {
            if (id != entry.Id)
            {
                return BadRequest();
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Entry>> PostEntry(Entry entry)
        {
            context.Entries.Add(entry);
            await context.SaveChangesAsync();

            return Ok(new Response { Status = "Success", Message = $"Entry created" });
        }

        // DELETE: api/Entries/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(long id)
        {
            var entry = await context.Entries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            context.Entries.Remove(entry);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool EntryExists(long id)
        {
            return context.Entries.Any(e => e.Id == id);
        }
    }
}
