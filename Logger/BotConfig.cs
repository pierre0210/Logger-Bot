using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Logger
{
    public class BotConfig
    {
        public string BotToken { get; set; } = string.Empty;
        public string OwnerGuild { get; set; } = string.Empty;

        public void InitConfig()
        {
            try
            {
                File.WriteAllText("example.config.json", JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented));
            }
            catch(Exception ex) 
            {
                Log.Error(ex.Message);
            }
            if(!File.Exists("config.json"))
            {
                Log.Error("File config.json doesn't exist.");
                Environment.Exit(3);
            }
            
            var config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            BotToken = config.BotToken;
            OwnerGuild = config.OwnerGuild;
        }
    }
}
