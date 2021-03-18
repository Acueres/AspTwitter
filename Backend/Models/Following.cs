using System;
using System.Collections.Generic;


namespace AspTwitter.Models
{
    public class Following
    {
        public uint Id { get; set; }

        public virtual User User { get; set; }
        public uint UserId { get; set; }

        public virtual User Follower { get; set; }
        public uint FollowerId { get; set; }
    }
}
