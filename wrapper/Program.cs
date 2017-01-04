using System;
using System.Net;
using System.Net.Http;
//using Newtonsoft.Json;
//using PSSniper;
using System.IO;

namespace ConsoleApplication
{
    public class HttpWrapper
    {
        public static void Main(string[] args)
        {
            HttpClient httpcli = new HttpClient();
            //PSSniper.Config config =  JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            //httpcli.BaseAddress = new Uri(args[0]);
            if (args.Length  >0 ) {
                httpcli.GetAsync(new Uri(args[0]));
            }

        }
    }
}
