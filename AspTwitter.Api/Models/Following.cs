using System;
using System.Collections.Generic;


namespace AspTwitter.Models
{
    public class Following
    {
        public int Id { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        public virtual User Follower { get; set; }
        public int FollowerId { get; set; }
    }
}
