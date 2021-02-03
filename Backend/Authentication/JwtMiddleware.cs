using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Text;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;


namespace AspTwitter.Authentication
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;
        private readonly AppSettings appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            this.next = next;
            this.appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserAuthentication userAuth)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                AttachUserToContext(context, userAuth, token);
            }

            await next(context);
        }

        private void AttachUserToContext(HttpContext context, IUserAuthentication userAuth, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Secret));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                long userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                context.Items["User"] = userAuth.GetUser(userId);
            }
            catch { }
        }
    }
}
