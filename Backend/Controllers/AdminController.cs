using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using AspTwitter.AppData;
using AspTwitter.Models;


namespace AspTwitter.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;

        public AdminController(AppDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [Route("home")]
        public async Task<IActionResult> Home()
        {
            ViewBag.UserCount = await context.Users.CountAsync();
            ViewBag.PostCount = await context.Entries.CountAsync();

            return View("Backend/Views/Admin/Home.cshtml");
        }

        [Route("users/{page?}")]
        public async Task<IActionResult> Users(string search, int page = 1)
        {
            var users = from x in context.Users select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = context.Users.Where(x => x.Name.ToLower().Contains(search) ||
                x.Username.ToLower().Contains(search) || x.Email.ToLower().Contains(search));
            }

            int n = 25;
            int count = await users.CountAsync();
            int pageCount = (int)Math.Ceiling((float)count / n);

            if (page < 1)
            {
                page = 1;
            }
            else if (page > pageCount)
            {
                page = pageCount;
            }

            var usersArr = await users.OrderByDescending(x => x.Id).ToArrayAsync();

            int start = (page - 1) * n;
            n = start + n > count ? count - start : n;

            if (usersArr.Length == 0)
            {
                ViewBag.Users = new List<User>();
            }
            else
            {
                ViewBag.Users = usersArr[start..(start + n)];
            }

            ViewBag.Page = page;
            ViewBag.PageCount = pageCount;

            return View("Backend/Views/Admin/Users.cshtml");
        }

        [HttpGet]
        [Route("users/{id}")]
        public async Task<IActionResult> UserEdit(uint id)
        {
            ViewBag.User = await context.Users.FindAsync(id);

            return View("Backend/Views/Admin/UserEdit.cshtml");
        }

        [Route("entries/{page?}")]
        public async Task<IActionResult> Entries(string search, int page = 1)
        {
            var entries = from x in context.Entries select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                entries = context.Entries.Where(x => x.Text.ToLower().Contains(search) ||
                x.Author.Username.ToLower().Contains(search));
            }

            var entriesArr = await entries.OrderByDescending(x => x.Id).ToArrayAsync();

            int n = 25;
            int count = await entries.CountAsync();
            int pageCount = (int)Math.Ceiling((float)count / n);

            if (page < 1)
            {
                page = 1;
            }
            else if (page > pageCount)
            {
                page = pageCount;
            }

            int start = (page - 1) * n;
            n = start + n > count ? count - start : n;

            if (entriesArr.Length == 0)
            {
                ViewBag.Entries = new List<Entry>();
            }
            else
            {
                ViewBag.Entries = entriesArr[start..(start + n)];
            }

            ViewBag.Page = page;
            ViewBag.PageCount = pageCount;

            return View("Backend/Views/Admin/Entries.cshtml");
        }
    }
}
