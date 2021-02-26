using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

using Xunit;

using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

using AspTwitter.Requests;
using AspTwitter.Models;
using AspTwitter.Authentication;


namespace AspTwitter.Tests
{
    public class UserTests : IClassFixture<WebAppFactory<Startup>>
    {
        private readonly HttpClient client;

        public UserTests(WebAppFactory<Startup> factory)
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

            await EditUsers(auth1, auth2);
            await DeleteUsers(auth1, auth2);
        }

        private async Task<AuthenticationResponse[]> CreateUsers()
        {
            string url = "api/authentication/register";

            RegisterRequest registerRequest = new RegisterRequest
            {
                Name = "User3",
                Username = "user3",
                Email = "test@email.com",
                Password = "testpassword3"
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
                Name = "User Delete",
                Username = "userdelete",
                Password = "testpassword"
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

        private async Task EditUsers(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);

            //Ensure correct empty data handling
            EditUserRequest editUserRequest = new EditUserRequest
            {
                Name = " ",
                About = ""
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{auth1.Id}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(editUserRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure successful user editing
            editUserRequest = new EditUserRequest
            {
                Name = " User 3 Edited ",
                About = " Test text "
            };

            request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{auth1.Id}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(editUserRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Ensure correct data handling
            response = await client.GetAsync($"api/Users/{auth1.Id}");
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            User editedUser = JsonConvert.DeserializeObject<User>(result);

            Assert.True(editedUser.Name == "User 3 Edited");
            Assert.True(editedUser.About == "Test text");

            //Ensure that authorized users cannot edit other users' data
            request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{auth2.Id}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(editUserRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        private async Task DeleteUsers(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Ensure that authorized users cannot delete other users
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/users/{auth2.Id}"));

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure user deletion
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth2.Token);
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/users/{auth2.Id}"));

            response.EnsureSuccessStatusCode();

            //Ensure that the user has been deleted
            response = await client.GetAsync($"api/users/{auth2.Id}");

            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}

