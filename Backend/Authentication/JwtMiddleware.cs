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

        public async Task Invoke(HttpContext context, IAuthenticationManager auth)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string adminToken = context.Request.Cookies["AdminAuthorization"];

            if (token is not null)
            {
                AttachUserToContext(context, auth, token);
            }

            if (adminToken is not null)
            {
                AttachUserToContext(context, auth, adminToken);
            }

            await next(context);
        }

        private void AttachUserToContext(HttpContext context, IAuthenticationManager auth, string token)
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
                int userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                context.Items["User"] = auth.GetUser(userId);
            }
            catch { }
        }
    }
}
