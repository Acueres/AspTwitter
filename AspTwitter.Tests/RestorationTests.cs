using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AspTwitter.AppData;
using AspTwitter.Authentication;
using AspTwitter.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AspTwitter.Tests;

public class RestorationTests
{
    [Fact]
    public async Task FreshDatabaseSupportsRegistrationLoginAndFirstTweet()
    {
        using var factory = new WebAppFactory<Startup>();
        using var client = factory.CreateClient();
        Assert.Empty((await client.GetFromJsonAsync<List<Entry>>("api/entries/partial/0"))!);

        var registration = await client.PostAsJsonAsync("api/authentication/register", new
        {
            Name = "Restored User", Username = "restored", Password = "testpassword"
        });
        registration.EnsureSuccessStatusCode();
        var auth = (await registration.Content.ReadFromJsonAsync<AuthenticationResponse>())!;
        Assert.True(auth.Id > 1); // The reserved admin is seeded on startup.
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));

        var login = await client.PostAsJsonAsync("api/authentication/login", new
        {
            Username = "restored", Password = "testpassword"
        });
        login.EnsureSuccessStatusCode();
        auth = (await login.Content.ReadFromJsonAsync<AuthenticationResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var post = await client.PostAsJsonAsync("api/entries", new { AuthorId = auth.Id, Text = "First tweet!" });
        post.EnsureSuccessStatusCode();
        var id = await post.Content.ReadFromJsonAsync<int>();

        var feed = (await client.GetFromJsonAsync<List<Entry>>("api/entries/partial/0"))!;
        var tweet = Assert.Single(feed);
        Assert.Equal(id, tweet.Id);
        Assert.Equal("First tweet!", tweet.Text);
        Assert.Equal("restored", tweet.Author.Username);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"api/users/{auth.Id}/avatar")).StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Add("Cookie", $"AdminAuthorization={auth.Token}");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("admin/test")).StatusCode);
    }

    [Fact]
    public async Task FeedPagesHaveNoGapsAndEndWithAnEmptyArray()
    {
        using var factory = new WebAppFactory<Startup>();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Entries.AddRange(Enumerable.Range(0, 76).Select(i => new Entry
            {
                AuthorId = 1, Text = $"Tweet {i}", Timestamp = new DateTime(2021, 5, 1).AddMinutes(i)
            }));
            await db.SaveChangesAsync();
        }

        var ids = new List<int>();
        foreach (var (page, count) in new[] { (0, 25), (1, 25), (2, 25), (3, 1), (4, 0), (int.MaxValue, 0) })
        {
            var entries = (await client.GetFromJsonAsync<List<Entry>>($"api/entries/partial/{page}"))!;
            Assert.Equal(count, entries.Count);
            ids.AddRange(entries.Select(e => e.Id));
        }
        Assert.Equal(76, ids.Distinct().Count());
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/register")]
    [InlineData("/_framework/blazor.webassembly.js")]
    [InlineData("/css/bootstrap/bootstrap.min.css")]
    [InlineData("/js/jquery-3.5.1.min.js")]
    public async Task ApiHostsTheRestoredClient(string path)
    {
        using var factory = new WebAppFactory<Startup>();
        using var client = factory.CreateClient();
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(await response.Content.ReadAsByteArrayAsync());
    }
}
