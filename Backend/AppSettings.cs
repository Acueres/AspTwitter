using Newtonsoft.Json;

using System.Collections.Generic;

using AspTwitter.Models;


namespace AspTwitter
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public List<AppModel> Apps { get; set; } = new List<AppModel>();

        public AppSettings()
        {
            string path = "Backend/AppData/apps.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                Apps = JsonConvert.DeserializeObject<List<AppModel>>(json);
            }
        }
    }
}
