using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

using System;
using System.Linq;

using AspTwitter.Models;


namespace AspTwitter.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool allowAnonymous = context.ActionDescriptor.EndpointMetadata.
                                     Any(x => x.GetType() == typeof(AllowAnonymousAttribute));

            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (allowAnonymous)
            {
                return;
            }

            if (descriptor.ControllerName == "Admin")
            {
                if ((User)context.HttpContext.Items["Admin"] is null)
                {
                    context.Result = new RedirectResult("admin/login");
                }

                return;
            }

            User user = (User)context.HttpContext.Items["User"];

            if (user is null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
