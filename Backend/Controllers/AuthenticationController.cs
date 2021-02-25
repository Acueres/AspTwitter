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
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequest request)
        {
            request.Username = request.Username.Replace(" ", string.Empty);
            request.Password = request.Password.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password))
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
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            //Clean whitespace
            request.Name = request.Name.Trim();
            request.Username = request.Username.Replace(" ", string.Empty);
            request.Password = request.Password.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(request.Name) ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest();
            }

            if (request.Email != null)
            {
                request.Email = request.Email.Replace(" ", string.Empty);

                //Ignore email if format is incorrect
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
