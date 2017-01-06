using System;
using System.Net;
using System.Net.Http;
//using Newtonsoft.Json;
//using PSSniper;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApplication
{
    public class HttpWrapper
    {
       
      public static async Task<string> GetStringFromUri(string uri)
{
     string result="";
     try {
      HttpClient h = new HttpClient();
      h.Timeout = TimeSpan.FromSeconds(2);
        result = await h.GetStringAsync(new Uri(uri));
     } catch {
     
     }
      return result;
    
}
       
        public static void Main(string[] args)
        {
            //HttpClient httpcli = new HttpClient();
            //httpcli.Timeout = TimeSpan.FromSeconds(1;)
            //PSSniper.Config config =  JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            //httpcli.BaseAddress = new Uri(args[0]);
            //Console.WriteLine(args[0]);
            

            if (args.Length  >0 ) {
                string abc =  (GetStringFromUri(args[0])).Result;
                
            }

        }
    }
}
