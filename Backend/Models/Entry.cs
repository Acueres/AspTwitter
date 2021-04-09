using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AspTwitter.Models
{
    public class Entry
    {
        public uint Id { get; set; }
        public DateTime Timestamp { get; set; }

        public uint LikeCount { get; set; }
        public uint RetweetCount { get; set; }
        public uint CommentCount { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual User Author { get; set; }
        public uint AuthorId { get; set; }

        public string Text { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual IList<Relationship> Relationships { get; set; } = new List<Relationship>();

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
    }
}
