using System;
using System.Collections.Generic;


namespace Frontend.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public int LikeCount { get; set; }
        public int RetweetCount { get; set; }
        public int CommentCount { get; set; }

        public virtual User Author { get; set; }
        public int AuthorId { get; set; }

        public string Text { get; set; }

        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
    }
}
