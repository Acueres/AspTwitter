using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AspTwitter.Models;
using AspTwitter.Requests;
using AspTwitter.AppData;

using AuthorizeAttribute = AspTwitter.Authentication.AuthorizeAttribute;


namespace AspTwitter.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;

        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        // GET: api/Users
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(uint id)
        {
            User user = await context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/Users/5/entries
        [AllowAnonymous]
        [HttpGet("{id}/entries")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Entries.Include(x => x.Author).Where(x => x.AuthorId == id).ToListAsync();
        }

        // GET: api/Users/5/avatar
        [AllowAnonymous]
        [HttpGet("{id}/avatar")]
        public async Task<IActionResult> GetAvatar(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            System.IO.FileStream image;

            string path = $"wwwroot/avatars/{id}.jpg";
            if (System.IO.File.Exists(path))
            {
                image = System.IO.File.OpenRead(path);
                return File(image, "image/jpeg");
            }

            image = System.IO.File.OpenRead($"wwwroot/avatars/default.png");
            return File(image, "image/jpeg");
        }

        // POST: api/Users/5/avatar
        [HttpPost("{id}/avatar")]
        [Consumes("multipart/form-data", "image/jpg", "image/png")]
        public async Task<IActionResult> PostAvatar(uint id, [FromForm(Name = "avatar")] IFormFile image)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            int mb = 1024 * 1024;

            if (image is null || image.Length > mb)
            {
                return BadRequest();
            }

            string path = $"wwwroot/Avatars/{id}.jpg";

            using var stream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
            await image.CopyToAsync(stream);


            return Ok();
        }

        // PATCH: api/Users/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PutUser(uint id, EditUserRequest request)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            request = ProcessEditRequest(request);
            if (request is null)
            {
                return BadRequest();
            }

            User user = await context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            if (user.Username != request.Username &&
                await context.Users.AnyAsync(e => e.Username == request.Username))
            {
                return Conflict();
            }

            user.Name = request.Name;
            user.Username = request.Username;
            user.About = request.About;

            context.Entry(user).State = EntityState.Modified;

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

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(uint id)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            string avatarPath = $"wwwroot/avatars/{id}.jpg";
            if (System.IO.File.Exists(avatarPath))
            {
                System.IO.File.Delete(avatarPath);
            }

            return Ok();
        }

        //GET: api/Users/5/retweets
        [AllowAnonymous]
        [HttpGet("{id}/retweets")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetRetweets(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Relationships.
                 Where(x => x.UserId == id && x.Type == RelationshipType.Retweet).Select(x => x.Entry).ToListAsync();
        }

        // GET: api/Users/5/favorites
        [AllowAnonymous]
        [HttpGet("{id}/favorites")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetFavorites(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Relationships.
                Where(x => x.UserId == id && x.Type == RelationshipType.Like).Select(x => x.Entry).ToListAsync();
        }

        // GET: api/Users/5/comments
        [AllowAnonymous]
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Comments.Where(x => x.AuthorId == id).ToListAsync();
        }

        //POST: api/Users/5/follow
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowUser(uint id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            uint followerId = ((User)HttpContext.Items["User"]).Id;

            if (followerId == id)
            {
                return BadRequest();
            }

            User follower = await context.Users.FindAsync(followerId);

            var following = await context.Followers.
                Where(x => x.FollowerId == followerId && x.UserId == id).FirstOrDefaultAsync();

            if (following is not null)
            {
                return BadRequest();
            }

            following = new()
            {
                UserId = id,
                FollowerId = followerId
            };

            context.Followers.Add(following);

            user.FollowerCount++;
            context.Entry(user).State = EntityState.Modified;

            follower.FollowingCount++;
            context.Entry(follower).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //DELETE: api/Users/5/unfollow
        [HttpDelete("{id}/unfollow")]
        public async Task<IActionResult> UnfollowUser(uint id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            uint followerId = ((User)HttpContext.Items["User"]).Id;

            if (followerId == id)
            {
                return BadRequest();
            }

            User follower = await context.Users.FindAsync(followerId);

            var following = await context.Followers.
                Where(x => x.FollowerId == followerId && x.UserId == id).FirstOrDefaultAsync();

            if (following is null)
            {
                return BadRequest();
            }

            context.Followers.Remove(following);

            user.FollowerCount--;
            context.Entry(user).State = EntityState.Modified;

            follower.FollowingCount--;
            context.Entry(follower).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Ok();
        }

        //GET: api/Users/5/followers
        [AllowAnonymous]
        [HttpGet("{id}/followers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Followers.Include(x => x.User).Where(x => x.UserId == id).Select(x => x.Follower).ToListAsync();
        }

        //GET: api/Users/5/following
        [AllowAnonymous]
        [HttpGet("{id}/following")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowings(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Followers.Include(x => x.Follower).Where(x => x.FollowerId == id).Select(x => x.User).ToListAsync();
        }

        //POST: api/Users/search
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers([FromBody] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest();
            }

            if (query.Contains('@'))
            {
                query = query.Replace("@", string.Empty);
                return await context.Users.Where(x => x.Username.ToLower().Contains(query)).ToListAsync();
            }

            return await context.Users.Where(x => x.Name.ToLower().Contains(query)).ToListAsync();
        }

        //GET: api/Users/5/recommended/3
        [AllowAnonymous]
        [HttpGet("{userId}/recommended/{count?}")]
        public async Task<ActionResult<IEnumerable<User>>> RecommendedUsers(uint userId, int count = 3)
        {
            if (count < 1)
            {
                return BadRequest();
            }

            int total = await context.Users.CountAsync();
            if (total <= count)
            {
                return null;
            }

            return await context.Users.OrderByDescending(x => x.FollowerCount).Include(x => x.Followers).
                Where(x => x.Id != userId && !x.Followers.Any(x => x.FollowerId == userId)).Take(count).ToListAsync();
        }

        private bool HasPermission(uint id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }

        private EditUserRequest ProcessEditRequest(EditUserRequest request)
        {
            if (request.Name is null || request.Username is null)
            {
                return null;
            }

            request.Name = request.Name.Trim();
            request.Username = request.Username.Replace(" ", string.Empty);

            if (request.Name == string.Empty || request.Username == string.Empty)
            {
                return null;
            }

            if (request.About != null)
            {
                request.About = request.About.Trim();
            }

            request.Name = Util.Truncate(request.Name, MaxLength.Name);
            request.Username = Util.Truncate(request.Username, MaxLength.Username);
            request.About = Util.Truncate(request.About, MaxLength.About);

            return request;
        }
    }
}
