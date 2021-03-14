using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

using Xunit;

using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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
            await LikeEntries(auth1, auth2);
            await RetweetEntries(auth1, auth2);
            await CommentEntries(auth1, auth2);
        }

        private async Task<AuthenticationResponse[]> CreateUsers()
        {
            RegisterRequest registerData = new()
            {
                Name = "User4",
                Username = "user4",
                Email = "test@email.com",
                Password = "testpassword4"
            };

            var auth1 = await CreateUser(registerData);
            Assert.True(auth1.Token != null);

            registerData = new()
            {
                Name = "User5",
                Username = "user5",
                Password = "testpassword5"
            };

            var auth2 = await CreateUser(registerData);
            Assert.True(auth2.Token != null);

            return new AuthenticationResponse[] { auth1, auth2 };
        }

        private async Task PostEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Ensure entry creation
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "text"
            };

            await CreateEntry(entryData);
           
            var response = await CreateEntry(new
            {
                AuthorId = "not a number",
                Text = "text"
            });
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure empty data handling
            entryData = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = " "
            };
            response = await CreateEntry(entryData);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure that users cannot post for other users
            entryData = new EntryRequest
            {
                AuthorId = auth2.Id,
                Text = "text"
            };
            response = await CreateEntry(entryData);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure text truncation in case of exceeding 256 symbol limit
            entryData = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = new string('*', (int)MaxLength.Entry + 10)
            };
            response = await CreateEntry(entryData);

            //Get entry and check text length
            uint entryId = await GetEntryId(response);
            Entry entry = await GetEntry(entryId);
            Assert.True(entry.Text.Length <= (int)MaxLength.Entry);
        }

        private async Task EditEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Add a test entry and get its id
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "text"
            };
            var response = await CreateEntry(entryData);
            uint entryId = await GetEntryId(response);

            //Ensure entry editing and data handling
            EntryRequest editEntryData = new()
            {
                AuthorId = auth1.Id,
                Text = " text edited "
            };
            response = await EditEntry(entryId, editEntryData);
            response.EnsureSuccessStatusCode();

            //Get entry and check its correctness
            Entry entry = await GetEntry(entryId);
            Assert.True(entry.Text == "text edited");

            //Ensure that authorized users cannot edit other users' entries
            SetUser(auth2);

            editEntryData = new EntryRequest
            {
                AuthorId = auth1.Id,
                Text = "new text"
            };
            response = await EditEntry(entryId, editEntryData);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        private async Task DeleteEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Add a test entry to delete it later
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "delete"
            };
            var response = await CreateEntry(entryData);
            uint entryId = await GetEntryId(response);

            //Ensure that authorized users cannot delete other users' entries
            SetUser(auth2);

            response = await DeleteEntry(entryId);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure entry deletion
            SetUser(auth1);

            response = await DeleteEntry(entryId);
            response.EnsureSuccessStatusCode();

            //Ensure that the entry has been deleted
            response = await client.GetAsync($"api/entries/{entryId}");
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure that it's impossible to delete a non-existent entry
            response = await DeleteEntry(42);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }

        private async Task LikeEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Create an entry and get its id
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "text"
            };
            var response = await CreateEntry(entryData);
            uint entryId = await GetEntryId(response);

            //Like the entry as the first user
            response = await AddLike(entryId);
            response.EnsureSuccessStatusCode();

            //Ensure that users cannot like the same entry more than once
            response = await AddLike(entryId);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure that users cannot like a non-existent tweet
            response = await AddLike(42);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Like the entry as the second user
            SetUser(auth2);

            response = await AddLike(entryId);
            response.EnsureSuccessStatusCode();

            //Ensure that all likes have been recorded
            Entry entry = await GetEntry(entryId);
            Assert.True(entry.LikeCount == 2);

            //Ensure that users cannot remove likes from non-existent tweets
            response = await RemoveLike(42);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Remove like as the second user
            response = await RemoveLike(entryId);
            response.EnsureSuccessStatusCode();

            //Ensure that like has been removed
            entry = await GetEntry(entryId);
            Assert.True(entry.LikeCount == 1);
        }

        private async Task RetweetEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Create an entry and get its id
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "text to retweet"
            };
            var response = await CreateEntry(entryData);
            uint entryId = await GetEntryId(response);

            //Retweet the entry as the second user
            SetUser(auth2);

            response = await client.PostAsync($"api/entries/{entryId}/retweet", null);
            response.EnsureSuccessStatusCode();

            //Ensure that users cannot retweet more than once
            response = await client.PostAsync($"api/entries/{entryId}/retweet", null);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure that users cannot retweet non-existent tweets
            response = await client.PostAsync($"api/entries/{42}/retweet", null);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure that the retweet has been recorded
            var entries = await GetUserEntries(auth2.Id);
            Assert.Contains(entries, x => x.Id == entryId);

            Entry entry = await GetEntry(entryId);
            Assert.True(entry.RetweetCount == 1);

            //Ensure that users cannot delete non-existent retweets
            response = await client.DeleteAsync($"api/entries/{42}/retweet");
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            //Ensure retweet deletion
            response = await client.DeleteAsync($"api/entries/{entryId}/retweet");
            response.EnsureSuccessStatusCode();

            //Check that the retweet has been deleted
            entries = await GetUserEntries(auth2.Id);
            Assert.DoesNotContain(entries, x => x.Id == entryId);

            entry = await GetEntry(entryId);
            Assert.True(entry.RetweetCount == 0);
        }

        private async Task CommentEntries(AuthenticationResponse auth1, AuthenticationResponse auth2)
        {
            //Create an entry and get its id
            SetUser(auth1);

            EntryRequest entryData = new()
            {
                AuthorId = auth1.Id,
                Text = "text to comment"
            };
            var response = await CreateEntry(entryData);
            uint entryId = await GetEntryId(response);

            //Ensure comment creation and get its id
            EntryRequest commentData = new()
            {
                AuthorId = auth1.Id,
                Text = "comment1"
            };

            response = await AddComment(entryId, commentData);
            response.EnsureSuccessStatusCode();
            uint commentId = await GetEntryId(response);

            //Ensure that users cannot add comments to non-existent entries
            response = await AddComment(42, commentData);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure that users cannot add comments using other user's id
            commentData = new()
            {
                AuthorId = auth2.Id,
                Text = "comment1"
            };
            response = await AddComment(entryId, commentData);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Add comment as the second user
            SetUser(auth2);

            commentData = new()
            {
                AuthorId = auth2.Id,
                Text = "comment2"
            };
            response = await AddComment(entryId, commentData);
            response.EnsureSuccessStatusCode();

            //Ensure that all comments have been recorded
            List<Comment> comments = await GetComments(entryId);
            Assert.True(comments.Count == 2);

            //Ensure that users cannot delete other users' comments
            response = await client.DeleteAsync($"api/entries/{entryId}/comments/{commentId}");
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            //Ensure that users cannot delete comments of non-existent entries
            SetUser(auth1);

            response = await client.DeleteAsync($"api/entries/{42}/comments/{commentId}");
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure that users cannot delete non-existent comments
            response = await client.DeleteAsync($"api/entries/{entryId}/comments/{42}");
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            //Ensure comment deletion
            response = await client.DeleteAsync($"api/entries/{entryId}/comments/{commentId}");
            response.EnsureSuccessStatusCode();

            //Ensure that comment deletion has been recorded
            comments = await GetComments(entryId);
            Assert.True(comments.Count == 1);
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

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(result);
        }

        private async Task<HttpResponseMessage> CreateEntry(object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/entries")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            return response;
        }

        private async Task<HttpResponseMessage> EditEntry(uint id, EntryRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/entries/{id}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            return response;
        }

        private async Task<HttpResponseMessage> DeleteEntry(uint id)
        {
            return await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/entries/{id}"));
        }

        private async Task<Entry> GetEntry(uint id)
        {
            var response = await client.GetAsync($"api/entries/{id}");

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Entry>(result);
        }

        private async Task<uint> GetEntryId(HttpResponseMessage response)
        {
            return uint.Parse(await response.Content.ReadAsStringAsync());
        }

        private async Task<HttpResponseMessage> AddLike(uint id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/entries/{id}/favorite");
            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> RemoveLike(uint id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/entries/{id}/favorite");
            return await client.SendAsync(request);
        }

        private async Task<List<Entry>> GetUserEntries(uint id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{id}/entries");
            var response = await client.SendAsync(request);

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Entry>>(result);
        }

        private async Task<List<Comment>> GetComments(uint id)
        {
            var response = await client.GetAsync($"api/entries/{id}/comments");

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Comment>>(result);
        }

        private async Task<HttpResponseMessage> AddComment(uint id, EntryRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/entries/{id}/comments")
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            return response;
        }
    }
}

