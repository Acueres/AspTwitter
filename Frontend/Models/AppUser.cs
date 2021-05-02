using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

using Newtonsoft.Json;


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
                User user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                SetData(user);

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
