using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    [Serializable]
    public class ServerConfig
    {
        public string dataPath;
        public string connectionString;
    }

    class ConfigManager
    {
        public static ServerConfig Config { get; private set; }

        public static void LoadConfig()
        {
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
