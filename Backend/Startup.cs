using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

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
            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "Frontend/Web-Vue";
            });

            services.AddEntityFrameworkSqlite();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(@"host=localhost;database=AspTwitter;username=postgres;password=postgres"));

            services.Configure<AppSettings>(Configuration.GetSection("JWT"));
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Map("/vue", mappedSpa =>
            {
                mappedSpa.UseSpa(spa =>
                {
                    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions()
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Frontend/Web-Vue"))
                    };
                    spa.Options.SourcePath = "Frontend/Web-Vue";
                });
            });
        }
    }
}
