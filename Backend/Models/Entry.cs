﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AspTwitter.Models
{
    public class Entry
    {
        public uint Id { get; set; }
        public DateTime Timestamp { get; set; }

        public uint LikesCount { get; set; }

        public virtual User Author { get; set; }
        public uint AuthorId { get; set; }

        public string Text { get; set; }

        [JsonIgnore]
        public virtual IList<Relationship> Relationships { get; set; } = new List<Relationship>();
    }
}
