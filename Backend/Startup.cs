using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;

using AspTwitter.AppData;
using AspTwitter.Authentication;

using System.IO;


namespace AspTwitter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite($"Data Source={Directory.GetCurrentDirectory()}/Backend/AppData/data.db"));

            services.Configure<AppSettings>(Configuration.GetSection("JWT"));
            services.AddScoped<IUserAuthentication, UserAuthentication>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors((x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
