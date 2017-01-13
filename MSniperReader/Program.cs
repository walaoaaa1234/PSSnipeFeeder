using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http;

namespace MsniperReader
{
    public class Program
    {
        private static HubConnection _connection;
        private static IHubProxy _msniperHub;
        private static string _msniperServiceUrl = "http://msniper.com/signalr";
        public static bool isConnected = false;
        private static object locker = new object();
        private static Config config = new Config();
        public static void Main(string[] args)
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\config.json"));
            ConnectMe();
            {
                Thread.Sleep(99999);
             } while (true) ;
        }

        private static void ConnectMe()
        {
            while (true)
            {
                try
                {
                    if (!isConnected)
                    {
                        Thread.Sleep(10000);
                        _connection = new HubConnection(_msniperServiceUrl, useDefaultUrl: false);
                        _connection.Protocol = Version.Parse("1.5");
                        _msniperHub = _connection.CreateHubProxy("msniperHub");
                        _connection.Received += Connection_Received;
                        _connection.Reconnecting += Connection_Reconnecting;
                        _connection.Closed += Connection_Closed;
                        _connection.Start().Wait();
                        Console.WriteLine("connected to msniper");
                        _msniperHub.Invoke("RecvIdentity");
                        isConnected = true;
                    }
                    break;
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message.ToString());
                    Thread.Sleep(500);
                }
            }



        }
        private static void Connection_Received(string obj)
        {
           // try
           // {
                HubData xx = _connection.JsonDeserializeObject<HubData>(obj);
                //Console.WriteLine(xx.Method);
                switch (xx.Method)
                {
                    case "RareList":
                        //Console.WriteLine("==>RareList Received");
                        break;
                    
                    case "NewPokemons":
                    var tmp = xx.List[0];
                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";

                    foreach (var Pokemon in tmp)
                    {
                        string PokemonName = Pokemon.PokemonName;
                        double PokemonIV = Convert.ToDouble(Pokemon.Iv, culture);
                        double latitude = Convert.ToDouble(Pokemon.Latitude, culture);
                        double longtitude = Convert.ToDouble(Pokemon.Longitude, culture);
                        ulong EncounterId = Pokemon.EncounterId;
                        string SpawnpointId = Pokemon.SpawnPointId;

                        //Console.WriteLine(String.Format("Pokemon: {0} IV: {1}",PokemonName, PokemonIV.ToString()));

                        bool requestsent = false;
                        foreach (filter filter in config.filters)
                        {
                            if (PokemonName.Contains(filter.namefilter) & (PokemonIV >= filter.minimumiv) & (!requestsent))
                                {
                                //msniper://Bulbasaur/14761440487074771357/31da1bab9f9/1.2869941788726442,103.7796544959606/56.92
                                string uri = config.PSSniperUrl + String.Format("/addpokemon/msniper://{0}/{1}/{2}/{3},{4}/{5}", PokemonName,EncounterId.ToString(),SpawnpointId,latitude.ToString(culture),longtitude.ToString(culture),PokemonIV.ToString(culture) );
                                Console.WriteLine(String.Format("Calling : {0}", uri));
                                try
                                {
                                    string result = "";
                                    requestsent = true;
                                    HttpClient h = new HttpClient();
                                    h.Timeout = TimeSpan.FromSeconds(2);
                                    result = h.GetStringAsync(new Uri(uri)).GetAwaiter().GetResult();
                                    requestsent = true;
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                //Console.WriteLine(String.Format("====================> Pokemon didn't met current  );  
                            }
                        }
                        
                        
                    }


                    break;
                    
                    case "Exceptions":
                        var defaultc = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        //Logger.Write("ERROR: " + xx.List.FirstOrDefault(), LogLevel.Service);
                        Console.ForegroundColor = defaultc;
                        break;
                }
            //}
            //catch (Exception e )
            //{
            //    Console.WriteLine(e.Message.ToString());
            //}
        }

        private static void Connection_Reconnecting()
        {
            isConnected = false;
            _connection.Stop(); //removing server cache
            ConnectMe();
            //Logger.Write("reconnecting", LogLevel.Service);
        }

        private static void Connection_Closed()
        {
            //Logger.Write("connection closed, trying to reconnect in 10secs", LogLevel.Service);
            ConnectMe();
        }
  

    }

     public class HubData
    {
        [JsonProperty("H")]
        public string HubName { get; set; }

        [JsonProperty("A")]
        public dynamic List { get; set; }

       // [JsonProperty("A")]
        //public List<List<string>> List { get; set; }

        [JsonProperty("M")]
        public string Method { get; set; }
    }

    public class filter
    {
        [JsonProperty("minimum_iv")]
        public double minimumiv { get; set; }

        [JsonProperty("name_filter")]
        public string namefilter { get; set; }

    }

    public class Config
    {
        [JsonProperty("PSSniperUrl")]
        public string PSSniperUrl;
        [JsonProperty("filters")]
        public List<filter> filters;

    }



}
