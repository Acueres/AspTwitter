using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

using System;
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
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private IAuthenticationManager auth;

        public AuthenticationController(AppDbContext context, IConfiguration configuration, IAuthenticationManager auth)
        {
            this.context = context;
            this.configuration = configuration;
            this.auth = auth;
        }

        // POST: api/Authentication/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequest request)
        {
            if (request.Username is null || request.Password is null)
            {
                return BadRequest();
            }

            request.Username = request.Username.Replace(" ", string.Empty);
            request.Password = request.Password.Replace(" ", string.Empty);

            if (request.Username == string.Empty ||
                request.Password == string.Empty)
            {
                return BadRequest();
            }

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

        // POST: api/Authentication/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request.Name is null || request.Username is null || request.Password is null)
            {
                return BadRequest();
            }

            //Clean whitespace
            request.Name = request.Name.Trim();
            request.Username = request.Username.Replace(" ", string.Empty);
            request.Password = request.Password.Replace(" ", string.Empty);

            if (request.Name == string.Empty ||
                request.Username == string.Empty ||
                request.Password == string.Empty)
            {
                return BadRequest();
            }

            if (ExceedsLength(request.Name, MaxLength.Name) ||
                ExceedsLength(request.Username, MaxLength.Username) ||
                ExceedsLength(request.Password, MaxLength.Password))
            {
                return BadRequest();
            }

            if (request.Email != null)
            {
                request.Email = request.Email.Replace(" ", string.Empty);

                //Ignore email if it exceeds max length or its format is incorrect
                if (ExceedsLength(request.Email, MaxLength.Email))
                {
                    request.Email = null;
                }

                try
                {
                    var address = new System.Net.Mail.MailAddress(request.Email);
                }
                catch
                {
                    request.Email = null;
                }
            }

            if (context.Users.Any(e => e.Username == request.Username))
            {
                return Conflict();
            }

            User user = new()
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

        //GET: api/Authentication/test
        [Authorize]
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok();
        }

        private bool ExceedsLength(string val, MaxLength length)
        {
            return val.Length > (int)length;
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
