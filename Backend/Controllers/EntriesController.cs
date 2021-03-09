using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AspTwitter.AppData;
using AspTwitter.Models;
using AspTwitter.Requests;
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
        public async Task<ActionResult<Entry>> GetEntry(uint id)
        {
            Entry entry = await context.Entries.FindAsync(id);

            if (entry is null)
            {
                return NotFound();
            }

            return entry;
        }

        // PUT: api/Entries/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntry(uint id, EntryRequest request)
        {
            if (!HasPermission(request.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (request.Text is null)
            {
                return BadRequest();
            }

            request.Text = request.Text.Trim();

            if (request.Text == string.Empty)
            {
                return BadRequest();
            }

            Entry entry = await context.Entries.FindAsync(id);

            if (entry is null)
            {
                return NotFound();
            }

            request.Text = Truncate(request.Text, MaxLength.Entry);
            entry.Text = request.Text;

            context.Entry(entry).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        // POST: api/Entries
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Entry>> PostEntry(EntryRequest request)
        {
            if (!HasPermission(request.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (request.Text is null)
            {
                return BadRequest();
            }

            request.Text = request.Text.Trim();

            if (request.Text == string.Empty)
            {
                return BadRequest();
            }

            request.Text = Truncate(request.Text, MaxLength.Entry);

            Entry entry = new()
            {
                AuthorId = request.AuthorId,
                Text = request.Text
            };

            context.Entries.Add(entry);
            await context.SaveChangesAsync();

            return Ok(entry.Id);
        }

        // DELETE: api/Entries/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(uint id)
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

        //POST api/Entries/5/favorite
        [Authorize]
        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> AddFavorite(uint id)
        {
            uint userId = ((User)HttpContext.Items["User"]).Id;
            Relationship relationship = await context.Relationships.Where(x =>
            x.Type == RelationshipType.Like &&
            x.UserId == userId &&
            x.EntryId == id).SingleOrDefaultAsync();

            if (relationship != null)
            {
                return BadRequest();
            }

            relationship = new()
            {
                Type = RelationshipType.Like,
                UserId = userId,
                EntryId = id
            };

            context.Relationships.Add(relationship);

            Entry entry = await context.Entries.FindAsync(id);
            entry.LikesCount++;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //DELETE api/Entries/5/favorite
        [Authorize]
        [HttpDelete("{id}/favorite")]
        public async Task<IActionResult> RemoveFavorite(uint id)
        {
            uint userId = ((User)HttpContext.Items["User"]).Id;
            Relationship relationship = await context.Relationships.Where(x =>
            x.Type == RelationshipType.Like &&
            x.UserId == userId &&
            x.EntryId == id).SingleOrDefaultAsync();

            if (relationship is null)
            {
                return BadRequest();
            }

            context.Relationships.Remove(relationship);

            Entry entry = await context.Entries.FindAsync(id);
            entry.LikesCount--;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        private string Truncate(string val, MaxLength length)
        {
            if (val.Length > (int)length)
            {
                return val.Substring(0, (int)length);
            }

            return val;
        }

        private bool HasPermission(uint id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }
    }
}
