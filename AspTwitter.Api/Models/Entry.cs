using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AspTwitter.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public int LikeCount { get; set; }
        public int RetweetCount { get; set; }
        public int CommentCount { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual User Author { get; set; }
        public int AuthorId { get; set; }

        public string Text { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual IList<Relationship> Relationships { get; set; } = new List<Relationship>();

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
    }
}
