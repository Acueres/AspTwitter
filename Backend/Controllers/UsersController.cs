﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<User>> GetUser(int id)
        {
            User user = await context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/Users/username/example
        [AllowAnonymous]
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            User user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            if (user is null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/Users/5/entries
        [AllowAnonymous]
        [HttpGet("{id}/entries")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries(int id)
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
        public async Task<IActionResult> GetAvatar(int id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.Avatar != null)
            {
                using System.IO.MemoryStream ms = new(user.Avatar);
                return File(user.Avatar, "image/jpeg");
            }

            System.IO.FileStream image = System.IO.File.OpenRead($"wwwroot/avatars/default.png");
            return File(image, "image/jpeg");
        }

        // POST: api/Users/5/avatar
        [HttpPost("{id}/avatar")]
        [Consumes("multipart/form-data", "image/jpg", "image/png")]
        public async Task<IActionResult> PostAvatar(int id, [FromForm(Name = "avatar")] IFormFile image)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
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

            using System.IO.MemoryStream ms = new();
            image.CopyTo(ms);
            user.Avatar = ms.ToArray();

            ms.Close();

            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok();
        }

        // PATCH: api/Users/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PutUser(int id, EditUserRequest request)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            request = Util.ProcessEditRequest(request);
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

        // PATCH: api/Users/5/edit-password
        [HttpPatch("{id}/edit-password")]
        public async Task<IActionResult> EditAdminPassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            User user = await context.Users.FindAsync(id);

            string oldPassword = request.OldPassword;
            string newPassword = request.NewPassword;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) ||
                oldPassword == newPassword || newPassword.Length < 5)
            {
                return BadRequest();
            }

            if (!Util.CompareHash(oldPassword, user.PasswordHash))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            user.PasswordHash = Util.Hash(newPassword);

            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!HasPermission(id) || id == 1)
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

            return Ok();
        }

        //GET: api/Users/5/retweets
        [AllowAnonymous]
        [HttpGet("{id}/retweets")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetRetweets(int id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Relationships.Include(x => x.Entry).Include(x => x.Entry.Author).
                 Where(x => x.UserId == id && x.Type == RelationshipType.Retweet).Select(x => x.Entry).ToListAsync();
        }

        // GET: api/Users/5/favorites
        [AllowAnonymous]
        [HttpGet("{id}/favorites")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetFavorites(int id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Relationships.Include(x => x.Entry).Include(x => x.Entry.Author).
                Where(x => x.UserId == id && x.Type == RelationshipType.Like).Select(x => x.Entry).ToListAsync();
        }

        // GET: api/Users/5/comments
        [AllowAnonymous]
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Comments.Where(x => x.AuthorId == id).ToListAsync();
        }

        //POST: api/Users/5/follow
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowUser(int id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            int followerId = ((User)HttpContext.Items["User"]).Id;

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
        public async Task<IActionResult> UnfollowUser(int id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            int followerId = ((User)HttpContext.Items["User"]).Id;

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
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(int id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Followers.Include(x => x.Follower).Where(x => x.UserId == id).Select(x => x.Follower).ToListAsync();
        }

        //GET: api/Users/5/following
        [AllowAnonymous]
        [HttpGet("{id}/following")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowings(int id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Followers.Include(x => x.User).Where(x => x.FollowerId == id).Select(x => x.User).ToListAsync();
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
        public async Task<ActionResult<IEnumerable<User>>> RecommendedUsers(int userId, int count = 3)
        {
            if (count < 1)
            {
                return BadRequest();
            }

            int total = await context.Users.CountAsync();
            if (total <= count)
            {
                return new List<User>();
            }

            return await context.Users.OrderByDescending(x => x.FollowerCount).Include(x => x.Followers).
                Where(x => x.Id != userId && !x.Followers.Any(x => x.FollowerId == userId)).Take(count).ToListAsync();
        }

        private bool HasPermission(int id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }
    }
}
