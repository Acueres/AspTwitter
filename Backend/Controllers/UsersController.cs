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
        private IUserAuthentication userAuthentication;

        public UsersController(AppDbContext context, IConfiguration configuration, IUserAuthentication userAuth)
        {
            this.context = context;
            this.configuration = configuration;
            userAuthentication = userAuth;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            User user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(long id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            context.Entry(user).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        [HttpPost]
        [Route("login")]
        public ActionResult Login([FromBody] AuthenticationRequest request)
        {
            User user = context.Users.Where(x => x.Username == request.Username).Single();

            if (user is null)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                       new Response { Status = "Error", Message = $"User \"{request.Username}\" does not exist" });
            }

            if (!CompareHash(request.Password, user.PasswordHash))
            {
                return StatusCode(StatusCodes.Status401Unauthorized,
                       new Response { Status = "Error", Message = $"Incorrect password" });
            }

            return Ok(userAuthentication.Authenticate(request));
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (context.Users.Any(e => e.Username == request.Username))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "User already exists" });
            }

            User user = new User()
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = Hash(request.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Ok(userAuthentication.Authenticate(user));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return context.Users.Any(e => e.Id == id);
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
