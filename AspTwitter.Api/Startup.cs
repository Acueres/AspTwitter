using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using AspTwitter.AppData;
using AspTwitter.Authentication;

namespace AspTwitter
{
    public class Startup
    {
        public Startup(IConfiguration configuration) { Configuration = configuration; }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                var connection = new SqliteConnectionStringBuilder(Configuration.GetConnectionString("DefaultConnection"));
                if (connection.DataSource != ":memory:" && !Path.IsPathRooted(connection.DataSource))
                    connection.DataSource = Path.Combine(env.ContentRootPath, connection.DataSource);
                if (connection.DataSource != ":memory:")
                    Directory.CreateDirectory(Path.GetDirectoryName(connection.DataSource)!);
                options.UseSqlite(connection.ToString());
            });
            services.AddOptions<AppSettings>()
                .Bind(Configuration.GetSection("JWT"))
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret)
                    && System.Text.Encoding.UTF8.GetByteCount(settings.Secret) >= 32,
                    "Configure JWT:Secret with a signing key of at least 32 bytes.")
                .ValidateOnStart();
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            // Preserve the old controllers' explicit validation of request fields.
            services.AddControllersWithViews(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback("api/{**path}", context =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                });
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
