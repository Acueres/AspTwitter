using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using System;
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
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly IAuthenticationManager auth;

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

            if (!Util.CompareHash(request.Password, user.PasswordHash))
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

            if (Util.ExceedsLength(request.Name, MaxLength.Name) ||
                Util.ExceedsLength(request.Username, MaxLength.Username) ||
                Util.ExceedsLength(request.Password, MaxLength.Password))
            {
                return BadRequest();
            }

            if (request.Email != null)
            {
                request.Email = request.Email.Replace(" ", string.Empty);

                //Ignore email if it exceeds max length or its format is incorrect
                if (Util.ExceedsLength(request.Email, MaxLength.Email))
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

            if (await context.Users.AnyAsync(e => e.Username == request.Username))
            {
                return Conflict();
            }

            User user = new()
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = Util.Hash(request.Password)
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
    }
}
