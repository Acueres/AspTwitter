using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Frontend.Models;


namespace Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<AppUser>();

            builder.Services.AddBlazoredLocalStorage();

            var host = builder.Build();

            var appUserService = host.Services.GetRequiredService<AppUser>();

            await appUserService.InitializeAsync(
                host.Services.GetRequiredService<ILocalStorageService>(),
                host.Services.GetRequiredService<HttpClient>()
                );

            await host.RunAsync();
        }
    }
}
