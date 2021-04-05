using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

using System;
using System.Linq;


namespace AspTwitter.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool allowAnonymous = context.ActionDescriptor.EndpointMetadata.
                                     Any(x => x.GetType() == typeof(AllowAnonymousAttribute));

            if (allowAnonymous) return;

            bool keyValid = false;
            try
            {
                keyValid = (bool)context.HttpContext.Items["ApiKeyValid"];
            }
            catch (NullReferenceException) { }

            if (!keyValid)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}