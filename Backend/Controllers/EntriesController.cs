using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using AspTwitter.AppData;
using AspTwitter.Models;
using AspTwitter.Requests;

using AuthorizeAttribute = AspTwitter.Authentication.AuthorizeAttribute;


namespace AspTwitter.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly AppDbContext context;

        public EntriesController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/Entries/partial/5
        [AllowAnonymous]
        [HttpGet("partial/{part}")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries(int part)
        {
            if (part < 0)
            {
                return BadRequest();
            }

            var res = await context.Entries.Include(x => x.Author).OrderByDescending(x => x.LikeCount).
                ThenByDescending(x => x.Timestamp).Take(1000).ToListAsync();

            int n = 25;
            int count = res.Count;
            n = n > count ? count : n;
            int cutoff = (int)Math.Ceiling((float)count / n);

            if (part + 1 > cutoff)
            {
                return NoContent();
            }

            int start = part * n;
            int end = start + n > count ? count - start : start + n;

            return Ok(res.GetRange(start, end));
        }

        //GET: api/Entries
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries()
        {
            return await context.Entries.ToListAsync();
        }

        // GET: api/Entries/5
        [AllowAnonymous]
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

        // PATCH: api/Entries/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditEntry(int id, EntryRequest request)
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

            if (request.Text is null)
            {
                return BadRequest();
            }

            request.Text = request.Text.Trim();

            if (request.Text == string.Empty)
            {
                return BadRequest();
            }

            request.Text = Util.Truncate(request.Text, MaxLength.Entry);
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

            request.Text = Util.Truncate(request.Text, MaxLength.Entry);

            Entry entry = new()
            {
                AuthorId = request.AuthorId,
                Text = request.Text,
                Timestamp = DateTime.UtcNow
            };

            context.Entries.Add(entry);
            await context.SaveChangesAsync();

            return Ok(entry.Id);
        }

        // DELETE: api/Entries/5
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
        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> AddFavorite(int id)
        {
            int userId = ((User)HttpContext.Items["User"]).Id;

            Entry entry = await context.Entries.FindAsync(id);
            if (entry is null)
            {
                return BadRequest();
            }

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

            entry.LikeCount++;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //DELETE api/Entries/5/favorite
        [HttpDelete("{id}/favorite")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            int userId = ((User)HttpContext.Items["User"]).Id;

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
            entry.LikeCount--;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //POST api/Entries/5/retweet
        [HttpPost("{id}/retweet")]
        public async Task<IActionResult> Retweet(int id)
        {
            int userId = ((User)HttpContext.Items["User"]).Id;

            Entry entry = await context.Entries.FindAsync(id);
            if (entry is null || entry.AuthorId == userId)
            {
                return BadRequest();
            }

            Relationship relationship = await context.Relationships.Where(x =>
            x.Type == RelationshipType.Retweet &&
            x.UserId == userId &&
            x.EntryId == id).SingleOrDefaultAsync();

            if (relationship != null)
            {
                return BadRequest();
            }

            relationship = new()
            {
                Type = RelationshipType.Retweet,
                UserId = userId,
                EntryId = id
            };

            context.Relationships.Add(relationship);

            entry.RetweetCount++;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //DELETE api/Entries/5/retweet
        [HttpDelete("{id}/retweet")]
        public async Task<IActionResult> RemoveRetweet(int id)
        {
            int userId = ((User)HttpContext.Items["User"]).Id;

            Relationship relationship = await context.Relationships.Where(x =>
            x.Type == RelationshipType.Retweet &&
            x.UserId == userId &&
            x.EntryId == id).SingleOrDefaultAsync();

            if (relationship is null)
            {
                return BadRequest();
            }

            context.Relationships.Remove(relationship);

            Entry entry = await context.Entries.FindAsync(id);
            entry.RetweetCount--;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //GET api/Entries/5/comments
        [AllowAnonymous]
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(uint id)
        {
            if (await context.Entries.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Comments.Include(x => x.Author).Include(x => x.Parent).
                Where(x => x.ParentId == id).ToListAsync();
        }

        //POST api/Entries/5/comments
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> PostComment(int id, EntryRequest request)
        {
            if (!HasPermission(request.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            Entry entry = await context.Entries.FindAsync(id);

            if (entry is null)
            {
                return NotFound();
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

            request.Text = Util.Truncate(request.Text, MaxLength.Comment);

            Comment comment = new()
            {
                AuthorId = request.AuthorId,
                ParentId = id,
                Text = request.Text,
                Timestamp = DateTime.UtcNow
            };

            context.Comments.Add(comment);

            entry.CommentCount++;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok(comment.Id);
        }

        //DELETE api/Entries/5/comments/5
        [HttpDelete("{entryId}/comments/{id}")]
        public async Task<IActionResult> DeleteEntry(uint entryId, uint id)
        {
            Entry entry = await context.Entries.FindAsync(entryId);
            if (entry is null)
            {
                return NotFound();
            }

            Comment comment = await context.Comments.FindAsync(id);

            if (comment is null)
            {
                return NotFound();
            }

            if (!HasPermission(comment.AuthorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            context.Comments.Remove(comment);

            entry.CommentCount--;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //POST: api/Entries/search
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Entry>>> SearchEntries([FromBody] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest();
            }

            return await context.Entries.Include(x => x.Author).Where(x => x.Text.ToLower().Contains(query)).Take(50).ToListAsync();
        }

        private bool HasPermission(int id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }
    }
}
