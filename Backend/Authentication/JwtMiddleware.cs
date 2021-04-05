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
            string apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();

            if (token is not null)
            {
                AttachUserToContext(context, auth, token);
            }
            if (apiKey is not null)
            {
                AttachKeyToContext(context, apiKey);
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
                uint userId = uint.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                context.Items["User"] = auth.GetUser(userId);
            }
            catch { }
        }

        private void AttachKeyToContext(HttpContext context, string token)
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
                string appName = jwtToken.Claims.First(x => x.Type == "appName").Value;
                uint generation = uint.Parse(jwtToken.Claims.First(x => x.Type == "generation").Value);
                bool keyValid = appSettings.Apps.Any(x => x.Name == appName) &&
                    appSettings.Apps.Where(x => x.Name == appName).First().Generation == generation;

                context.Items["ApiKeyValid"] = keyValid;
            }
            catch { }
        }
    }
}
