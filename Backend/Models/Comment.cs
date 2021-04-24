using System;


namespace AspTwitter.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual User Author { get; set; }
        public int AuthorId { get; set; }

        public virtual Entry Parent { get; set; }
        public int ParentId { get; set; }

        public string Text { get; set; }
    }
}
