using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

using Xunit;

using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

using AspTwitter.Requests;
using AspTwitter.Models;
using AspTwitter.Authentication;


namespace AspTwitter.Tests
{
    public class AuthenticationTests : IClassFixture<WebAppFactory<Startup>>
    {
        private readonly HttpClient client;

        public AuthenticationTests(WebAppFactory<Startup> factory)
        {
            client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Test()
        {
            await UserRegistration();
            await RegistrationDataHandling();
            await UserLogin();
        }

        private async Task UserRegistration()
        {
            string url = "api/authentication/register";

            //Ensure user creation
            RegisterRequest registerRequest = new RegisterRequest
            {
                Name = "User1",
                Username = "user1",
                Email = "test@email.com",
                Password = "testpassword1"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Ensure username uniqueness condition
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Conflict);
        }

        private async Task RegistrationDataHandling()
        {
            string url = "api/authentication/register";

            //Add dirty data
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new RegisterRequest
                {
                    Name = "  User 2 ",
                    Username = " user 2  ",
                    Email = "email",
                    Password = " test pass  word 2 "
                }), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            var authResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(result);

            Assert.True(authResponse.Name == "User 2");
            Assert.True(authResponse.Username == "user2");
            Assert.True(authResponse.Token != null);

            //Ensure that incorrect email format is ignored
            response = await client.GetAsync($"api/Users/{authResponse.Id}");
            response.EnsureSuccessStatusCode();

            result = await response.Content.ReadAsStringAsync();
            User user = JsonConvert.DeserializeObject<User>(result);

            Assert.True(user.Email is null);

            //Ensure that empty data is rejected
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new RegisterRequest
                {
                    Name = "  ",
                    Username = "",
                    Password = " "
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        private async Task UserLogin()
        {
            string url = "api/authentication/login";

            //Ensure user login and password cleanup from previous request
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new AuthenticationRequest
                {
                    Username = "user2",
                    Password = "testpassword2"
                }), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Ensure null data handling
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new AuthenticationRequest
                {
                    Username = null,
                    Password = " "
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure username handling
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new AuthenticationRequest
                {
                    Username = "usernone",
                    Password = "testpassword"
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure password handling
            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(
                new AuthenticationRequest
                {
                    Username = "user1",
                    Password = "testpassword2"
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
        }
    }
}
