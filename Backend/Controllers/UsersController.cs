﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AspTwitter.Models;
using AspTwitter.Requests;
using AspTwitter.AppData;
using AspTwitter.Authentication;


namespace AspTwitter.Controllers
{
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        // GET: api/Users/5
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
        [HttpGet("{id}/entries")]
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            var retweets = await context.Relationships.
                Where(x => x.UserId == id && x.Type == RelationshipType.Retweet).ToListAsync();
            var retweetIds = retweets.Select(x => x.EntryId).ToArray();
            var timestamps = retweets.Select(x => x.Timestamp).ToArray();

            var res = await context.Entries.Where(x => x.AuthorId == id || retweetIds.Contains(x.Id)).ToListAsync();

            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].AuthorId != id)
                {
                    int index = Array.IndexOf(retweetIds, res[i].AuthorId);
                    if (index != -1)
                    {
                        res[i].Timestamp = timestamps[index];
                    }
                }
            }

            return res;
        }

        // GET: api/Users/5/avatar
        [HttpGet("{id}/avatar")]
        public IActionResult GetAvatar(uint id)
        {
            string path = $"{System.IO.Directory.GetCurrentDirectory()}/Backend/AppData/Avatars/{id}.jpg";
            if (System.IO.File.Exists(path))
            {
                var image = System.IO.File.OpenRead(path);
                return File(image, "image/jpeg");
            }
            else
            {
                var image = System.IO.File.OpenRead($"{System.IO.Directory.GetCurrentDirectory()}/Backend/AppData/Avatars/default.png");
                return File(image, "image/jpeg");
            }
        }

        // POST: api/Users/5/avatar
        [Authorize]
        [HttpPost("{id}/avatar")]
        [Consumes("multipart/form-data", "image/jpg", "image/png")]
        public async Task<IActionResult> PostAvatar(uint id, [FromForm(Name = "avatar")] IFormFile image)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            int mb = 1024 * 1024;

            if (image != null && image.Length > mb)
            {
                return BadRequest();
            }

            try
            {
                string path = $"{System.IO.Directory.GetCurrentDirectory()}/Backend/AppData/Avatars/{id}.jpg";

                using var stream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
                await image.CopyToAsync(stream);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        // PUT: api/Users/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(uint id, EditUserRequest request)
        {
            if (request.Name is null)
            {
                return BadRequest();
            }

            request.Name = request.Name.Trim();

            if (request.Name == string.Empty)
            {
                return BadRequest();
            }

            if (request.About != null)
            {
                request.About = request.About.Trim();
            }

            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            User user = await context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            request.Name = Truncate(request.Name, MaxLength.Name);
            request.About = Truncate(request.About, MaxLength.About);

            user.Name = request.Name;
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
        [Authorize]
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

            string avatarPath = $"{System.IO.Directory.GetCurrentDirectory()}/Backend/AppData/Avatars/{id}.jpg";
            if (System.IO.File.Exists(avatarPath))
            {
                System.IO.File.Delete(avatarPath);
            }

            return Ok();
        }

        // GET: api/Users/5/favorites
        [HttpGet("{id}/favorites")]
        public async Task<ActionResult<IEnumerable<uint>>> GetFavorites(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Relationships.
                Where(x => x.UserId == id && x.Type == RelationshipType.Like).Select(x => x.EntryId).ToListAsync();
        }

        // GET: api/Users/5/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(uint id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Comments.Where(x => x.AuthorId == id).ToListAsync();
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
