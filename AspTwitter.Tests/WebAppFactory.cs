using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AspTwitter.AppData;

namespace AspTwitter.Tests
{
    public class WebAppFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            connection.Open();
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            });
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) connection.Dispose();
        }
    }
}
