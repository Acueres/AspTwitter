using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

using System;
using System.Threading.Tasks;

using AspTwitter.AppData;
using AspTwitter.Authentication;
using AspTwitter.Models;
using AspTwitter.Requests;


namespace AspTwitter.Controllers
{
    [AdminAuthorize]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly IAuthenticationManager auth;

        public AdminController(AppDbContext context, IConfiguration configuration, IAuthenticationManager auth)
        {
            this.context = context;
            this.configuration = configuration;
            this.auth = auth;
        }

        // GET: admin/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok();
        }

        // GET: admin/set-password
        [AllowAnonymous]
        [HttpGet("set-password")]
        public async Task<ActionResult<bool>> SetPassword()
        {
            User admin = await context.Users.FindAsync(1);
            return admin.PasswordHash == "null";
        }

        // POST: admin/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string password)
        {
            User admin = await context.Users.FindAsync(1);
            bool setPassword = admin.PasswordHash == "null";

            if (string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            if (setPassword)
            {
                if (password.Length < 5)
                {
                    return BadRequest();
                }

                admin.PasswordHash = Util.Hash(password);

                context.Entry(admin).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }

            if (!Util.CompareHash(password, admin.PasswordHash))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            CookieBuilder authCookieBuilder = new()
            {
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Expiration = new TimeSpan(8, 0, 0)
            };

            var authCookieOptions = authCookieBuilder.Build(HttpContext);
            string token = auth.Authenticate(admin).Token;
            Response.Cookies.Append("AdminAuthorization", token, authCookieOptions);

            return Ok();
        }

        // POST: admin/users/login
        [HttpPost("users/login")]
        public async Task<ActionResult> LoginAsUser([FromBody] AuthenticationRequest request)
        {
            if (request.Username is null)
            {
                return BadRequest();
            }

            request.Username = request.Username.Replace(" ", string.Empty);

            if (request.Username == string.Empty)
            {
                return BadRequest();
            }

            User user = await context.Users.SingleOrDefaultAsync(x => x.Username == request.Username);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(auth.Authenticate(request));
        }

        // PATCH: admin/Entries/5
        [HttpPatch("entries/{id}")]
        public async Task<IActionResult> EditEntry(int id, [FromBody] EntryRequest request)
        {
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

        // DELETE: admin/Entries/5
        [HttpDelete("entries/{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            Entry entry = await context.Entries.FindAsync(id);

            if (entry is null)
            {
                return NotFound();
            }

            context.Entries.Remove(entry);
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
