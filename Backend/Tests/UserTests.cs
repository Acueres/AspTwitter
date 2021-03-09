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
            RegisterRequest registerData = new()
            {
                Name = "User3",
                Username = "user3",
                Email = "test@email.com",
                Password = "testpassword3"
            };
            var auth1 = await CreateUser(registerData);
            Assert.True(auth1.Token != null);

            registerData = new()
            {
                Name = "User Delete",
                Username = "userdelete",
                Password = "testpassword"
            };
            var auth2 = await CreateUser(registerData);
            Assert.True(auth2.Token != null);

            return new AuthenticationResponse[] { auth1, auth2 };
        }

        private async Task EditUsers(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            SetUser(auth1);

            //Ensure correct empty data handling
            EditUserRequest editUserData = new()
            {
                Name = " ",
                About = ""
            };
            var response = await EditUser(auth1.Id, editUserData);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure successful user editing
            editUserData = new()
            {
                Name = " User 3 Edited ",
                About = " Test text "
            };
            response = await EditUser(auth1.Id, editUserData);
            response.EnsureSuccessStatusCode();

            User editedUser = await GetUser(auth1.Id);
            Assert.True(editedUser.Name == "User 3 Edited");
            Assert.True(editedUser.About == "Test text");

            //Ensure that authorized users cannot edit other users' data
            response = await EditUser(auth2.Id, editUserData);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        private async Task DeleteUsers(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Ensure that authorized users cannot delete other users
            SetUser(auth1);

            var response = await DeleteUser(auth2.Id);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure user deletion
            SetUser(auth2);

            response = await DeleteUser(auth2.Id);
            response.EnsureSuccessStatusCode();

            //Ensure that the user has been deleted
            response = await client.GetAsync($"api/users/{auth2.Id}");
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }

        private void SetUser(AuthenticationResponse auth)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        }

        private async Task<AuthenticationResponse> CreateUser(RegisterRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/authentication/register")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(result);
        }

        private async Task<HttpResponseMessage> EditUser(uint id, EditUserRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{id}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            return response;
        }

        private async Task<User> GetUser(uint id)
        {
            var response = await client.GetAsync($"api/users/{id}");
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(result);
        }

        private async Task<HttpResponseMessage> DeleteUser(uint id)
        {
            return await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/users/{id}"));
        }
    }
}

