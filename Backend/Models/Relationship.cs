﻿using System;


namespace AspTwitter.Models
{
    public enum RelationshipType
    {
        Like,
        Retweet
    }

    public class Relationship
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }

        public RelationshipType Type { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        public virtual Entry Entry { get; set; }
        public int EntryId { get; set; }
    }
}
