﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AspTwitter.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string About { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        [JsonIgnore]
        public virtual IList<Entry> Entries { get; set; } = new List<Entry>();
    }
}
