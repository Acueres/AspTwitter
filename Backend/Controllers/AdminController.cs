using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using AspTwitter.AppData;
using AspTwitter.Authentication;
using AspTwitter.Models;


namespace AspTwitter.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly IAuthenticationManager auth;
        private AppSettings appSettings;

        public AdminController(AppDbContext context, IConfiguration configuration, IAuthenticationManager auth, IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.configuration = configuration;
            this.auth = auth;
            this.appSettings = appSettings.Value;

            string path = "Backend/AppData/apps.json";
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, "[]");
            }
        }

        public IActionResult ToClient()
        {
            return Redirect("vue");
        }

        [Route("home")]
        public async Task<IActionResult> Home()
        {
            ViewBag.UserCount = await context.Users.CountAsync();
            ViewBag.PostCount = await context.Entries.CountAsync();

            return View("Backend/Views/Admin/Home.cshtml");
        }

        [Route("users/{page?}")]
        public async Task<IActionResult> Users(string search = null, string orderBy = "Id", bool ascending = false, int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.Order = orderBy;
            ViewBag.Ascending = ascending;

            IQueryable<User> users = from x in context.Users select x;
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

            User[] usersArr = orderBy switch
            {
                "Followers" => await users.OrderByDescending(x => x.FollowerCount).ToArrayAsync(),
                "Following" => await users.OrderByDescending(x => x.FollowingCount).ToArrayAsync(),
                _ => await users.OrderByDescending(x => x.Id).ToArrayAsync(),
            };

            if (ascending)
            {
                Array.Reverse(usersArr);
            }

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

            return View("Backend/Views/Admin/Users/Users.cshtml");
        }

        [Route("users/edit/{id}")]
        public async Task<IActionResult> EditUser(uint id, string name, string username, string email, string about, IFormFile avatar)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (string.IsNullOrEmpty(name))
            {
                ViewBag.User = user;
                return View("Backend/Views/Admin/Users/EditUser.cshtml");
            }

            if (!string.IsNullOrEmpty(name))
            {
                user.Name = name;
            }

            if (!string.IsNullOrEmpty(username))
            {
                username = username.Replace(" ", string.Empty);
                user.Username = username;
            }

            if (!string.IsNullOrEmpty(email) && email.Contains('@'))
            {
                email = email.Replace(" ", string.Empty);
                user.Email = email;
            }

            if (!string.IsNullOrEmpty(about))
            {
                user.About = about;
            }

            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            int mb = 1024 * 1024;
            if (avatar is not null && avatar.Length <= mb)
            {
                string path = $"wwwroot/Avatars/{user.Id}.jpg";

                using var stream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
                await avatar.CopyToAsync(stream);
            }

            ViewBag.User = user;
            return RedirectToAction("Users");
        }

        [Route("users/create")]
        public async Task<IActionResult> CreateUser(string name, string username, string email, string password)
        {
            if (await context.Users.AnyAsync(x => x.Username == username) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                return View("Backend/Views/Admin/Users/CreateUser.cshtml");
            }

            username = username.Replace(" ", string.Empty);
            password = password.Replace(" ", string.Empty);

            if (email is not null && !email.Contains('@'))
            {
                email = null;
            }

            User user = new()
            {
                Name = name,
                Username = username,
                Email = email,
                PasswordHash = Util.Hash(password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        [Route("users/delete")]
        public async Task<IActionResult> DeleteUser(uint id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            string avatarPath = $"wwwroot/avatars/{id}.jpg";
            if (System.IO.File.Exists(avatarPath))
            {
                System.IO.File.Delete(avatarPath);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Route("entries/{page?}")]
        public async Task<IActionResult> Entries(string search = null, string orderBy = "Id", bool ascending = false, int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.Order = orderBy;
            ViewBag.Ascending = ascending;

            IQueryable<Entry> entries = from x in context.Entries.Include(x => x.Author) select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                entries = context.Entries.Include(x => x.Author).Where(x => x.Text.ToLower().Contains(search) ||
                x.Author.Username.ToLower().Contains(search));
            }

            Entry[] entriesArr = orderBy switch
            {
                "Size" => await entries.OrderByDescending(x => x.Text.Length).ToArrayAsync(),
                "Date" => await entries.OrderByDescending(x => x.Timestamp).ToArrayAsync(),
                "Likes" => await entries.OrderByDescending(x => x.LikeCount).ToArrayAsync(),
                "Retweets" => await entries.OrderByDescending(x => x.RetweetCount).ToArrayAsync(),
                "Comments" => await entries.OrderByDescending(x => x.CommentCount).ToArrayAsync(),
                _ => await entries.OrderByDescending(x => x.Id).ToArrayAsync(),
            };

            if (ascending)
            {
                Array.Reverse(entriesArr);
            }

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

            return View("Backend/Views/Admin/Entries/Entries.cshtml");
        }

        [Route("entries/edit/{id}")]
        public async Task<IActionResult> EditEntry(uint id, string text)
        {
            Entry entry = await context.Entries.FindAsync(id);
            if (entry is null || string.IsNullOrEmpty(text))
            {
                ViewBag.Entry = entry;
                return View("Backend/Views/Admin/Entries/EditEntry.cshtml");
            }

            if (text is not null)
            {
                text = Util.Truncate(text, MaxLength.Entry);
                entry.Text = text;
            }

            context.Entry(entry).State = EntityState.Modified;
            await context.SaveChangesAsync();

            ViewBag.Entry = entry;

            return RedirectToAction("Entries");
        }

        [Route("entries/create")]
        public async Task<IActionResult> CreateEntry(string text, string username)
        {
            ViewBag.Username = username;
            User author = await context.Users.Where(x => x.Username == username).SingleOrDefaultAsync();
            if (string.IsNullOrEmpty(text) || author is null)
            {
                return View("Backend/Views/Admin/Entries/CreateEntry.cshtml");
            }

            Entry entry = new()
            {
                Text = text,
                AuthorId = author.Id,
                Timestamp = DateTime.UtcNow
            };

            context.Entries.Add(entry);
            await context.SaveChangesAsync();

            return RedirectToAction("Entries");
        }

        [Route("entries/delete")]
        public async Task<IActionResult> DeleteEntry(uint id)
        {
            Entry entry = await context.Entries.FindAsync(id);
            if (entry is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            context.Entries.Remove(entry);
            await context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Route("apps")]
        public IActionResult Apps()
        {
            ViewBag.Apps = appSettings.Apps;

            return View("Backend/Views/Admin/Apps/Apps.cshtml");
        }

        [Route("apps/create")]
        public async Task<IActionResult> RegisterApp(string name, string info, string path)
        {
            List<AppModel> apps = appSettings.Apps;

            if (apps.Any(x => x.Name == name) || string.IsNullOrEmpty(info))
            {
                return View("Backend/Views/Admin/Apps/RegisterApp.cshtml");
            }

            AppModel appData = new()
            {
                Name = name,
                Info = info,
                Key = auth.GetAppToken(name, 0),
                Path = path,
                Generation = 0
            };

            apps.Add(appData);
            await SaveApps(apps);
            await UpdateKey(appData.Path, appData.Key);

            return RedirectToAction("Apps");
        }

        [Route("apps/{name}/edit")]
        public async Task<IActionResult> EditApp(string name, string info, string path)
        {
            List<AppModel> apps = appSettings.Apps;
            var app = apps.Find(x => x.Name == name);

            if (app is null)
            {
                return RedirectToAction("Apps");
            }

            ViewBag.App = app;

            if (info is null)
            {
                return View("Backend/Views/Admin/Apps/EditApp.cshtml");
            }

            int index = apps.FindIndex(x => x.Name == name);
            apps[index].Info = info;
            apps[index].Path = path;
            await SaveApps(apps);

            return RedirectToAction("Apps");
        }

        [Route("apps/{name}/delete")]
        public async Task<IActionResult> DeleteApp(string name)
        {
            List<AppModel> apps = appSettings.Apps;
            if (!apps.Any(x => x.Name == name))
            {
                return RedirectToAction("Apps");
            }

            int index = apps.FindIndex(x => x.Name == name);
            apps.RemoveAt(index);
            await SaveApps(apps);

            return RedirectToAction("Apps");
        }

        [Route("apps/{name}/issue")]
        public async Task<IActionResult> IssueAppKey(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction("Apps");
            }

            List<AppModel> apps = appSettings.Apps;
            if (!apps.Any(x => x.Name == name))
            {
                return RedirectToAction("Apps");
            }

            int index = apps.FindIndex(x => x.Name == name);
            apps[index].Generation++;
            apps[index].Key = auth.GetAppToken(name, apps[index].Generation);

            await SaveApps(apps);
            await UpdateKey(apps[index].Path, apps[index].Key);

            return RedirectToAction("Apps");
        }

        private async Task SaveApps(List<AppModel> data)
        {
            string path = "Backend/AppData/apps.json";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(path, json);
        }

        private async Task UpdateKey(string path, string key)
        {
            string fullPath = path + "/api-key.js";
            if (System.IO.File.Exists(fullPath))
            {
                string text = $"var apiKey = '{key}'";
                await System.IO.File.WriteAllTextAsync(fullPath, text);
            }
        }
    }
}
