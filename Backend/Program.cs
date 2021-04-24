using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace AspTwitter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    if (env != "Development")
                    {
                        string port = Environment.GetEnvironmentVariable("PORT");
                        webBuilder.UseUrls("http://*:" + port);
                    }
                });
    }
}
