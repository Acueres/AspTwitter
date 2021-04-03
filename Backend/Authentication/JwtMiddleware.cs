using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

using AspTwitter.Models;


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
                await AttachKeyToContext(context, apiKey);
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

        private async Task AttachKeyToContext(HttpContext context, string token)
        {
            try
            {
                var apps = await GetApps();

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

                context.Items["ApiKey"] = appSettings.Apps.Contains(appName) &&
                                          apps.Find(x => x.Name == appName).Key == token;
            }
            catch { }
        }

        private async Task<List<AppModel>> GetApps()
        {
            string path = "Backend/AppData/apps.json";
            string json = await System.IO.File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<List<AppModel>>(json);
        }
    }
}
