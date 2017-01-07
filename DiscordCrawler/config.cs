using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PSSniperDiscordCrawler
{
   public class channel {
       [JsonProperty("server_name")]
       public string servername { get; set; }
       [JsonProperty("channel_name")]
       public string channelname { get; set; }
       [JsonProperty("parseregex")]
       public string parseregex { get; set; }
       
       [JsonProperty("minimum_iv")]
       public double minimumiv { get; set; }

       public ulong Channelid  { get; set; }
   }     

        public class Config      
    {
        [JsonProperty("user_token")]
        public string user_token;

        [JsonProperty("PSSniperUrl")]
        public string PSSniperUrl; 
        [JsonProperty("channels")]
        public List<channel> channels;
        
    }
}