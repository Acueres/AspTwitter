namespace AspTwitter.Models
{
    public enum RelationshipType
    {
        Like,
        Retweet,
        Comment
    }

    public class Relationship
    {
        public uint Id { get; set; }

        public RelationshipType Type { get; set; }

        public virtual User User { get; set; }
        public uint UserId { get; set; }

        public virtual Entry Entry { get; set; }
        public uint EntryId { get; set; }
    }
}
