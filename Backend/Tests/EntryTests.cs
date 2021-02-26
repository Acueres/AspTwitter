using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

using Xunit;

using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

using AspTwitter.AppData;
using AspTwitter.Requests;
using AspTwitter.Models;
using AspTwitter.Authentication;


namespace AspTwitter.Tests
{
    public class EntryTests : IClassFixture<WebAppFactory<Startup>>
    {
        private readonly HttpClient client;

        public EntryTests(WebAppFactory<Startup> factory)
        {
            client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Test()
        {
            //Register two users and get their authentication responses for further testing
            AuthenticationResponse auth1, auth2;
            var authData = await CreateUsers();
            auth1 = authData[0];
            auth2 = authData[1];

            await PostEntries(auth1, auth2);
            await EditEntries(auth1, auth2);
            await DeleteEntries(auth1, auth2);
        }

        private async Task<AuthenticationResponse[]> CreateUsers()
        {
            string url = "api/authentication/register";

            RegisterRequest registerRequest = new RegisterRequest
            {
                Name = "User4",
                Username = "user4",
                Email = "test@email.com",
                Password = "testpassword4"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            var auth1 = JsonConvert.DeserializeObject<AuthenticationResponse>(result);
            Assert.True(auth1.Token != null);

            registerRequest = new RegisterRequest
            {
                Name = "User5",
                Username = "user5",
                Password = "testpassword5"
            };

            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            result = await response.Content.ReadAsStringAsync();
            var auth2 = JsonConvert.DeserializeObject<AuthenticationResponse>(result);
            Assert.True(auth2.Token != null);

            return new AuthenticationResponse[] { auth1, auth2 };
        }

        private async Task PostEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            string url = "api/entries";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);

            //Ensure entry creation
            EntryRequest entryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = "text"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Ensure incorrect author id handling
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    AuthorId = "not a number",
                    Text = "text"
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure empty data handling
            entryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = " "
            };

            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure that users cannot post for other users
            entryRequest = new EntryRequest
            {
                AuthorId = auth2.Id,
                Text = "text"
            };

            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure text truncation in case of exceeding 256 symbol limit
            entryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = new string('*', (int)MaxLength.Entry + 10)
            };

            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Get entry and check text length
            long entryId = long.Parse(await response.Content.ReadAsStringAsync());
            response = await client.GetAsync($"api/entries/{entryId}");
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(result);

            Assert.True(entry.Text.Length <= (int)MaxLength.Entry);
        }

        private async Task EditEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);

            //Add a test entry and get its id
            EntryRequest entryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = "text"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/entries")
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            long entryId = long.Parse(await response.Content.ReadAsStringAsync());

            //Ensure entry editing and data handling
            EntryRequest editEntryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = " text edited "
            };

            request = new HttpRequestMessage(HttpMethod.Put, $"api/entries/{entryId}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(editEntryRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Get entry and check its correctness
            response = await client.GetAsync($"api/entries/{entryId}");
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(result);

            Assert.True(entry.Text == "text edited");

            //Ensure that authorized users cannot edit other users' entries
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth2.Token);

            editEntryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = "new text"
            };

            request = new HttpRequestMessage(HttpMethod.Put, $"api/entries/{entryId}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(editEntryRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        private async Task DeleteEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);

            //Add a test entry to delete it later
            EntryRequest entryRequest = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = "delete"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/entries")
            {
                Content = new StringContent(JsonConvert.SerializeObject(entryRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            long entryId = long.Parse(await response.Content.ReadAsStringAsync());

            //Ensure that authorized users cannot delete other users' entries
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth2.Token);

            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/entries/{entryId}"));

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure entry deletion
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);

            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/entries/{entryId}"));

            response.EnsureSuccessStatusCode();

            //Ensure that the user has been deleted
            response = await client.GetAsync($"api/entries/{entryId}");

            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure that it's impossible to delete a non-existent entry
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/entries/42"));

            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}

