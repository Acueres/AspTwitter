﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

using AspTwitter.AppData;
using AspTwitter.Models;
using AspTwitter.Requests;


namespace AspTwitter.Authentication
{
    public interface IAuthenticationManager
    {
        AuthenticationResponse Authenticate(AuthenticationRequest request);
        AuthenticationResponse Authenticate(User user);
        User GetUser(int id);
    }

    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;


        public AuthenticationManager(AppDbContext context, IConfiguration configuration)
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

            string token = UserJwtToken(user);

            return new AuthenticationResponse(user, token);
        }

        public AuthenticationResponse Authenticate(User user)
        {
            if (user is null)
            {
                return null;
            }

            string token = UserJwtToken(user);

            return new AuthenticationResponse(user, token);
        }

        public User GetUser(int id)
        {
            return context.Users.Find(id);
        }

        private string UserJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
