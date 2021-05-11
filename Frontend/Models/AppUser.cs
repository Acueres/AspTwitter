using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using System.Net.Http.Json;

using Newtonsoft.Json;
using Blazored.LocalStorage;


namespace Frontend.Models
{
    public class AppUser: User
    {
        public bool Logged { get; set; }
        public bool AdminRights { get; set; }
        public string Token { get; set; }

        public async Task SetLogin(AuthenticationResponse auth, HttpClient client)
        {
            var response = await client.GetAsync($"api/users/{auth.Id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string result = await response.Content.ReadAsStringAsync();
                User user = JsonConvert.DeserializeObject<User>(result);

                SetData(user);

                Logged = true;
                Token = auth.Token;
            }
        }

        public async Task InitializeAsync(ILocalStorageService localStorage, HttpClient client)
        {
            var response = await client.GetAsync("admin/test");
            AdminRights = response.StatusCode == HttpStatusCode.OK;

            var auth = await localStorage.GetItemAsync<AuthenticationResponse>("auth");
            if (auth is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

                response = await client.GetAsync("api/authentication/test");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return;
                }

                await SetLogin(auth, client);

                Favorites = await client.GetFromJsonAsync<List<Entry>>($"api/users/{Id}/favorites");
                Retweets = await client.GetFromJsonAsync<List<Entry>>($"api/users/{Id}/retweets");
            }
        }

        public async Task Logout(ILocalStorageService localStorage)
        {
            bool adminRights = AdminRights;

            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].SetValue(this, null);
            }

            AdminRights = adminRights;

            await localStorage.RemoveItemAsync("auth");
        }

        public async Task FollowUser(User user, HttpClient client)
        {
            if (!Logged) return;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await client.PostAsync($"api/users/{user.Id}/follow", null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Following.Add(user);
                FollowingCount++;
            }
        }

        public async Task UnfollowUser(User user, HttpClient client)
        {
            if (!Logged) return;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            if (Following.Any(e => e.Id == user.Id))
            {
                var response = await client.DeleteAsync($"api/users/{user.Id}/unfollow");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Following.Remove(user);
                    FollowingCount--;
                }

            }
        }

        public async Task FavoriteEntry(Entry entry, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await client.PostAsync($"api/entries/{entry.Id}/favorite", null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Favorites.Add(entry);
                entry.LikeCount++;
            }
        }

        public async Task UnfavoriteEntry(Entry entry, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await client.DeleteAsync($"api/entries/{entry.Id}/unfavorite");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (Favorites.Any(e => e.Id == entry.Id))
                {
                    Favorites.Remove(entry);
                }
                entry.LikeCount--;
            }
        }

        public async Task AddRetweet(Entry entry, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await client.PostAsync($"api/entries/{entry.Id}/retweet", null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Retweets.Add(entry);
                entry.RetweetCount++;
            }
        }

        public async Task RemoveRetweet(Entry entry, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await client.DeleteAsync($"api/entries/{entry.Id}/remove-retweet");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (Retweets.Any(e => e.Id == entry.Id))
                {
                    Retweets.Remove(entry);
                }
                entry.RetweetCount--;
            }
        }

        private void SetData(User user)
        {
            Type type = user.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (var prop in properties)
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(user, null), null);
            }
        }
    }
}
