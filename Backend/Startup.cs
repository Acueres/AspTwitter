using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;

using AspTwitter.AppData;
using AspTwitter.Authentication;

using System;
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

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string connectionStr = @"host=localhost;database=AspTwitter;username=postgres;password=postgres";

            if (env == "Production")
            {
                var connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                connectionUrl = connectionUrl.Replace("postgres://", string.Empty);
                var userPass = connectionUrl.Split("@")[0];
                var hostPortDb = connectionUrl.Split("@")[1];
                var hostPort = hostPortDb.Split("/")[0];
                var db = hostPortDb.Split("/")[1];
                var user = userPass.Split(":")[0];
                var pass = userPass.Split(":")[1];
                var host = hostPort.Split(":")[0];
                var port = hostPort.Split(":")[1];
                connectionStr = $"Server={host};Port={port};User Id={user};Password={pass};Database={db}";
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionStr));

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
