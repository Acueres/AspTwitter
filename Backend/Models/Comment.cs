using System;


namespace AspTwitter.Models
{
    public class Comment
    {
        public uint Id { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual User Author { get; set; }
        public uint AuthorId { get; set; }

        public virtual Entry Parent { get; set; }
        public uint ParentId { get; set; }

        public string Text { get; set; }
    }
}
