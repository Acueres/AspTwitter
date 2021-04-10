using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AspTwitter.Models
{
    public class User
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string Email { get; set; }

        public string About { get; set; }

        public DateTime DateJoined { get; set; }

        public uint FollowingCount { get; set; }
        public uint FollowerCount { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string PasswordHash { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public virtual IList<Entry> Entries { get; set; } = new List<Entry>();

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public virtual IList<Relationship> Relationships { get; set; } = new List<Relationship>();

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public virtual IList<Following> Following { get; set; } = new List<Following>();

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public virtual IList<Following> Followers { get; set; } = new List<Following>();
    }
}
