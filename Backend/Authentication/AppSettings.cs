using System.Collections.Generic;


namespace AspTwitter.Authentication
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public List<string> Apps { get; set; } = new List<string>();
    }
}
