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
            //Ensure user creation
            RegisterRequest registerData = new()
            {
                Name = "User1",
                Username = "user1",
                Email = "test@email.com",
                Password = "testpassword1"
            };
            var response = await RegisterUser(registerData);
            response.EnsureSuccessStatusCode();

            //Ensure username uniqueness condition
            response = await RegisterUser(registerData);
            Assert.True(response.StatusCode == HttpStatusCode.Conflict);
        }

        private async Task RegistrationDataHandling()
        {
            //Ensure that whitespace is handled
            RegisterRequest registerData = new()
            {
                Name = "  User 2 ",
                Username = " user 2  ",
                Email = "email",
                Password = " test pass  word 2 "
            };
            var response = await RegisterUser(registerData);
            response.EnsureSuccessStatusCode();

            var auth = await GetAuthData(response);
            User user = await GetUser(auth.Id);

            Assert.True(user.Name == "User 2");
            Assert.True(user.Username == "user2");
            Assert.True(auth.Token != null);

            //Ensure that incorrect email format is ignored
            Assert.True(user.Email is null);

            //Ensure that empty data is rejected
            registerData = new()
            {
                Name = "  ",
                Username = "",
                Password = " "
            };
            response = await RegisterUser(registerData);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        private async Task UserLogin()
        {
            //Ensure user login and password cleanup from previous request
            AuthenticationRequest loginData = new()
            {
                Username = "user2",
                Password = "testpassword2"
            };
            var response = await LoginUser(loginData);
            response.EnsureSuccessStatusCode();

            //Ensure empty data handling
            loginData = new()
            {
                Username = null,
                Password = " "
            };
            response = await LoginUser(loginData);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure username handling
            loginData = new()
            {
                Username = "usernone",
                Password = "testpassword"
            };
            response = await LoginUser(loginData);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure password handling
            loginData = new()
            {
                Username = "user1",
                Password = "testpassword2"
            };
            response = await LoginUser(loginData);
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
        }

        private async Task<HttpResponseMessage> RegisterUser(RegisterRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/authentication/register")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> LoginUser(AuthenticationRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/authentication/login")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            return await client.SendAsync(request);
        }

        private async Task<AuthenticationResponse> GetAuthData(HttpResponseMessage response)
        {
            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(result);
        }

        private async Task<User> GetUser(uint id)
        {
            var response = await client.GetAsync($"api/users/{id}");
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(result);
        }
    }
}
