using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using AspTwitter.AppData;
using AspTwitter.Authentication;
using AspTwitter.Models;

using AuthorizeAttribute = AspTwitter.Authentication.AuthorizeAttribute;


namespace AspTwitter.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
        public IActionResult ToApp()
        {
            return Redirect("app");
        }

        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login(string password)
        {
            User admin = await context.Users.Where(x => x.Username == "admin").FirstOrDefaultAsync();
            bool setPassword = admin.PasswordHash == "null";
            ViewBag.SetPassword = setPassword;

            if (string.IsNullOrEmpty(password))
            {
                return View("Backend/Views/Admin/Login.cshtml");
            }

            if (setPassword && password.Length >= 5)
            {
                admin.PasswordHash = Util.Hash(password);

                context.Entry(admin).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }

            if (Util.CompareHash(password, admin.PasswordHash))
            {
                CookieBuilder authCookieBuilder = new()
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Expiration = new TimeSpan(8, 0, 0)
                };

                var authCookieOptions = authCookieBuilder.Build(HttpContext);

                string token = auth.Authenticate(admin).Token;
                Response.Cookies.Append("AdminAuthorization", token, authCookieOptions);

                return RedirectToAction("Home");
            }

            return View("Backend/Views/Admin/Login.cshtml");
        }

        [Route("download")]
        public async Task<ActionResult> DownloadData(string type)
        {
            string data = type switch
            {
                "Users" => JsonConvert.SerializeObject(
                    await context.Users.ToListAsync(),
                    Formatting.Indented),
                _ => JsonConvert.SerializeObject(
                    await context.Entries.ToListAsync(),
                    Formatting.Indented)
            };

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            var output = new FileContentResult(bytes, "application/octet-stream")
            {
                FileDownloadName = $"asp_twitter-{type.ToLower()}.json"
            };

            return output;
        }

        [Route("home")]
        public async Task<IActionResult> Home()
        {
            ViewBag.Admin = await context.Users.Where(x => x.Username == "admin").FirstOrDefaultAsync();
            ViewBag.UserCount = await context.Users.CountAsync();
            ViewBag.PostCount = await context.Entries.CountAsync();

            return View("Backend/Views/Admin/Home.cshtml");
        }

        [Route("admin/password")]
        public async Task<IActionResult> EditAdminPassword(string oldPassword, string newPassword)
        {
            User admin = await context.Users.Where(x => x.Username == "admin").FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || oldPassword == newPassword ||
                newPassword.Length < 5 || !Util.CompareHash(oldPassword, admin.PasswordHash))
            {
                return View("Backend/Views/Admin/EditAdminPassword.cshtml");
            }

            admin.PasswordHash = Util.Hash(newPassword);

            context.Entry(admin).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return RedirectToAction("Home");
        }

        [Route("users/{page?}")]
        public async Task<IActionResult> Users(string search = null, string orderBy = "Id", bool ascending = false, int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.Order = orderBy;
            ViewBag.Ascending = ascending;

            IQueryable<User> users = from x in context.Users.Include(x => x.Comments) select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(x => x.Name.ToLower().Contains(search) ||
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

            if (!string.IsNullOrEmpty(username) && user.Username != "admin")
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
                PasswordHash = Util.Hash(password),
                DateJoined = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        [Route("users/delete")]
        public async Task<IActionResult> DeleteUser(uint id)
        {
            User user = await context.Users.FindAsync(id);
            if (user is null || user.Username == "admin")
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
                entries = entries.Where(x => x.Text.ToLower().Contains(search) ||
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

        [Route("entries/{id}/edit")]
        public async Task<IActionResult> EditEntry(uint id, string text)
        {
            Entry entry = await context.Entries.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);

            if (entry is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (string.IsNullOrEmpty(text))
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

        [Route("users/{username}/comments/{page?}")]
        public async Task<IActionResult> UserComments(string username, string search = null, string orderBy = "Id", bool ascending = false, int page = 1)
        {
            User author = await context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();
            if (author is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            ViewBag.Author = author;
            ViewBag.Search = search;
            ViewBag.Order = orderBy;
            ViewBag.Ascending = ascending;

            IQueryable<Comment> comments = from x in context.Comments where x.AuthorId == author.Id select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                comments = comments.Where(x => x.Text.ToLower().Contains(search));
            }

            Comment[] commentsArr = orderBy switch
            {
                "EntryId" => await comments.OrderByDescending(x => x.ParentId).ToArrayAsync(),
                "Size" => await comments.OrderByDescending(x => x.Text.Length).ToArrayAsync(),
                "Date" => await comments.OrderByDescending(x => x.Timestamp).ToArrayAsync(),
                _ => await comments.OrderByDescending(x => x.Id).ToArrayAsync(),
            };

            if (ascending)
            {
                Array.Reverse(commentsArr);
            }

            int n = 25;
            int count = await comments.CountAsync();
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

            if (commentsArr.Length == 0)
            {
                ViewBag.Comments = new List<Comment>();
            }
            else
            {
                ViewBag.Comments = commentsArr[start..(start + n)];
            }

            ViewBag.Page = page;
            ViewBag.PageCount = pageCount;

            return View("Backend/Views/Admin/Comments/UserComments.cshtml");
        }

        [Route("entries/{id}/comments/{page?}")]
        public async Task<IActionResult> EntryComments(uint id, string search = null, string orderBy = "Id", bool ascending = false, int page = 1)
        {
            Entry entry = await context.Entries.FindAsync(id);
            if (entry is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            ViewBag.Entry = entry;
            ViewBag.Search = search;
            ViewBag.Order = orderBy;
            ViewBag.Ascending = ascending;

            IQueryable<Comment> comments = from x in context.Comments where x.ParentId == entry.Id select x;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                comments = comments.Where(x => x.Text.ToLower().Contains(search));
            }

            Comment[] commentsArr = orderBy switch
            {
                "AuthorId" => await comments.OrderByDescending(x => x.AuthorId).ToArrayAsync(),
                "Size" => await comments.OrderByDescending(x => x.Text.Length).ToArrayAsync(),
                "Date" => await comments.OrderByDescending(x => x.Timestamp).ToArrayAsync(),
                _ => await comments.OrderByDescending(x => x.Id).ToArrayAsync(),
            };

            if (ascending)
            {
                Array.Reverse(commentsArr);
            }

            int n = 25;
            int count = await comments.CountAsync();
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

            if (commentsArr.Length == 0)
            {
                ViewBag.Comments = new List<Comment>();
            }
            else
            {
                ViewBag.Comments = commentsArr[start..(start + n)];
            }

            ViewBag.Page = page;
            ViewBag.PageCount = pageCount;

            return View("Backend/Views/Admin/Comments/EntryComments.cshtml");
        }

        [Route("comments/create")]
        public async Task<IActionResult> AddComment(string text, string username, int entryId)
        {
            ViewBag.Username = username;
            ViewBag.EntryId = entryId;
            User author = await context.Users.Where(x => x.Username == username).SingleOrDefaultAsync();
            Entry entry = await context.Entries.FindAsync(entryId);
            if (string.IsNullOrEmpty(text) || author is null || entry is null)
            {
                return View("Backend/Views/Admin/Comments/AddComment.cshtml");
            }

            Comment comment = new()
            {
                Text = text,
                AuthorId = author.Id,
                ParentId = entryId,
                Timestamp = DateTime.UtcNow
            };

            context.Comments.Add(comment);

            entry.CommentCount++;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return RedirectToAction("EntryComments", new { Id = entryId });
        }

        [Route("comments/{id}/edit")]
        public async Task<IActionResult> EditComment(uint id, string text)
        {
            Comment comment = await context.Comments.FindAsync(id);
            if (comment is null || string.IsNullOrEmpty(text))
            {
                ViewBag.Comment = comment;
                return View("Backend/Views/Admin/Comments/EditComment.cshtml");
            }

            if (text is not null)
            {
                text = Util.Truncate(text, MaxLength.Comment);
                comment.Text = text;
            }

            context.Entry(comment).State = EntityState.Modified;
            await context.SaveChangesAsync();

            ViewBag.Comment = comment;

            return RedirectToAction("EntryComments", new { Id = comment.ParentId });
        }

        [Route("comments/delete")]
        public async Task<IActionResult> DeleteComment(uint id)
        {
            Comment comment = await context.Comments.FindAsync(id);
            if (comment is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            Entry entry = await context.Entries.FindAsync(comment.ParentId);
            if (entry is null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            context.Comments.Remove(comment);

            entry.CommentCount--;
            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
