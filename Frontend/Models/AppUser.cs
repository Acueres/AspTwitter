﻿using System;
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
        public string Token { get; set; }

        public async Task SetLogin(AuthenticationResponse auth, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var response = await client.GetAsync("api/authentication/test");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return;
            }

            response = await client.GetAsync($"api/users/{auth.Id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string result = await response.Content.ReadAsStringAsync();
                User user = JsonConvert.DeserializeObject<User>(result);

                SetData(user);

                Entries = await client.GetFromJsonAsync<List<Entry>>($"api/users/{Id}/entries");

                Favorites = await client.GetFromJsonAsync<List<Entry>>($"api/users/{Id}/favorites");
                Comments = await client.GetFromJsonAsync<List<Comment>>($"api/users/{Id}/comments");

                Following = await client.GetFromJsonAsync<List<User>>($"api/users/{Id}/following");
                Followers = await client.GetFromJsonAsync<List<User>>($"api/users/{Id}/followers");

                Logged = true;
                Token = auth.Token;
            }
        }

        public async Task Initialize(ILocalStorageService localStorage, HttpClient client)
        {
            var auth = await localStorage.GetItemAsync<AuthenticationResponse>("auth");

            if (auth is not null)
            {
                await SetLogin(auth, client);
            }
        }

        public async Task Logout(ILocalStorageService localStorage)
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].SetValue(this, null);
            }

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
