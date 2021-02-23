using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

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
        private IAuthenticationManager auth;

        public UsersController(AppDbContext context, IConfiguration configuration, IAuthenticationManager auth)
        {
            this.context = context;
            this.configuration = configuration;
            this.auth = auth;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
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
        public async Task<ActionResult<IEnumerable<Entry>>> GetEntries(long id)
        {
            if (await context.Users.FindAsync(id) is null)
            {
                return NotFound();
            }

            return await context.Entries.Where(x => x.AuthorId == id).ToListAsync();
        }

        // GET: api/Users/5/avatar
        [HttpGet("{id}/avatar")]
        public IActionResult GetAvatar(long id)
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
        public async Task<IActionResult> PostAvatar(long id, [FromForm(Name = "avatar")] IFormFile image)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            string path = $"{System.IO.Directory.GetCurrentDirectory()}/Backend/AppData/Avatars/{id}.jpg";

            using var stream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
            await image.CopyToAsync(stream);

            return Ok();
        }

        // PUT: api/Users/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(long id, EditUserRequest request)
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

            if (request.Name != null && request.Name != "")
            {
                user.Name = request.Name;
            }

            user.About = request.About;


            context.Entry(user).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                throw;
            }

            return Ok();
        }

        // POST: api/Users/login
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequest request)
        {
            User user = await context.Users.Where(x => x.Username == request.Username).SingleOrDefaultAsync();

            if (user is null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (!CompareHash(request.Password, user.PasswordHash))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return Ok(auth.Authenticate(request));
        }

        // POST: api/Users/register
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (context.Users.Any(e => e.Username == request.Username))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            User user = new User()
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = Hash(request.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Ok(auth.Authenticate(user));
        }

        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            if (!HasPermission(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool HasPermission(long id)
        {
            return ((User)HttpContext.Items["User"]).Id == id;
        }

        private string Hash(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string key = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{10000}.{Convert.ToBase64String(salt)}.{key}";
        }

        private bool CompareHash(string password, string hash)
        {
            string[] hashParts = hash.Split('.');

            int iterations = Convert.ToInt32(hashParts[0]);
            byte[] salt = Convert.FromBase64String(hashParts[1]);
            byte[] key = Convert.FromBase64String(hashParts[2]);

            byte[] keyToCheck = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: iterations,
                numBytesRequested: 32);

            return key.SequenceEqual(keyToCheck);
        }
    }
}
