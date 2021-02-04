using System;

namespace AspTwitter.Models
{
    public class Entry
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }

        public User Author { get; set; }
        public long AuthorId { get; set; }

        public string Text { get; set; }


        public Entry()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
