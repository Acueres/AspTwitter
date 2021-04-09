using System.IO;

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
                    webBuilder.ConfigureKestrel(options => { });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
