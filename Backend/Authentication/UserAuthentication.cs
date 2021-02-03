using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

using AspTwitter.AppData;
using AspTwitter.Models;


namespace AspTwitter.Authentication
{
    public interface IUserAuthentication
    {
        AuthenticationResponse Authenticate(AuthenticationRequest request);
        AuthenticationResponse Authenticate(User user);
        User GetUser(long id);
    }

    public class UserAuthentication : IUserAuthentication
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;


        public UserAuthentication(AppDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public AuthenticationResponse Authenticate(AuthenticationRequest request)
        {
            User user = context.Users.Where(x => x.Username == request.Username).Single();

            if (user is null)
            {
                return null;
            }

            string token = GetJwtToken(user);

            return new AuthenticationResponse(user, token);
        }

        public AuthenticationResponse Authenticate(User user)
        {
            if (user is null)
            {
                return null;
            }

            string token = GetJwtToken(user);

            return new AuthenticationResponse(user, token);
        }

        public User GetUser(long id)
        {
            return context.Users.Find(id);
        }

        private string GetJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
