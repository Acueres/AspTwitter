using System;


namespace AspTwitter.Models
{
    public class AppModel
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string Key { get; set; }
        public string ConfigPath { get; set; }
        public DateTime KeyExpires { get; set; }
    }
}
