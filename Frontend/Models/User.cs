using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Frontend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }

        public string Email { get; set; }

        public string About { get; set; }

        public DateTime DateJoined { get; set; }

        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }

        public virtual IList<Entry> Entries { get; set; } = new List<Entry>();

        public virtual IList<Entry> Favorites { get; set; } = new List<Entry>();

        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();

        public virtual IList<User> Following { get; set; } = new List<User>();

        public virtual IList<User> Followers { get; set; } = new List<User>();
    }
}
