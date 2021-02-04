using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AspTwitter.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        public IList<Entry> Entries { get; set; } = new List<Entry>();
    }
}
