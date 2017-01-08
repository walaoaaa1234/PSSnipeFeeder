using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Globalization;

namespace PSSniperDiscordCrawler
{
    public class Program
    {
        
            // Create a DiscordClient with WebSocket support
            private Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory()+@"\config.json"));
            private DiscordSocketClient client;
            public ulong Guildid  = 0;
            public ulong ChannelID =0;
            
        
        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();
        public async Task Run()
    {   
            //Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory()+@"\config.json"));
            
            
            client = new DiscordSocketClient();
            string token = config.user_token;
            // Hook into the MessageReceived event on DiscordSocketClient
            client.MessageReceived += async (message) =>
            {   // Check to see if the Message Content is "!ping"
                if (message.Content == "!ping")
                    // Send 'pong' back to the channel the message was sent in
                    await message.Channel.SendMessageAsync("pong");
            };

            Console.WriteLine("Connecting to Discord.");
            // Configure the client to use a Bot token, and use our token
            await client.LoginAsync(TokenType.User, token);
            // Connect the client to Discord's gateway
            await client.ConnectAsync();
            client.MessageReceived += HandleCommand;
            Console.WriteLine("Validating servers and channels");
            foreach (channel record  in config.channels) {
                var server = client.Guilds.Where(s=>s.Name == record.servername).FirstOrDefault();
                if (server != null) {
                    var channel = server.Channels.Where(c=>c.Name == record.channelname).FirstOrDefault();
                    if (channel !=null) {
                        record.Channelid = channel.Id;
                    } else {
                        Console.WriteLine (String.Format("====>Channel {0} hasn't been found on server {1}",record.channelname,record.servername));
                    }

                } else {
                    Console.WriteLine (String.Format("==>Server {0} hasn't been found",record.servername));
                }

            }
            // Block this task until the program is exited.
            await Task.Delay(-1);
            


    }

        public async Task HandleCommand(SocketMessage messageParam)
	{
        var message = messageParam as SocketUserMessage;
        if (message == null) return;
            PSSniperDiscordCrawler.channel configchannel = config.channels.Where(c=>c.Channelid == message.Channel.Id).FirstOrDefault();
            if ( configchannel  != null) {
                //Console.WriteLine(String.Format("{0}",configchannel.parseregex));
                Console.WriteLine(String.Format("Server: {0} Channel:  {1} Message:  {2}",configchannel.servername, configchannel.channelname, message.Content));
                Regex parserregex = new Regex(configchannel.parseregex);
                Match m = parserregex.Match(message.Content);
                //m = parserregex.Match("Exeggutor 100IV 40.013137,-75.106145 818CP L10 Zen Headbutt/Psychic");
                if ( m.Success ) {
                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    string PokemonName = m.Groups["name"].Value;
                    double PokemonIV = Convert.ToDouble(m.Groups["IV"].Value,culture);
                    string latitude = m.Groups["latitude"].Value; 
                    string longtitude = m.Groups["longtitude"].Value; 
                    Console.WriteLine(String.Format("======> Name: {0} IV {1} Coordinates: {2},{3}" , m.Groups["name"].Value,m.Groups["IV"].Value,m.Groups["latitude"].Value,m.Groups["longtitude"].Value));
                    bool requestsent = false;
                    foreach (filter filter in configchannel.filters) {
                        if ( PokemonName.Contains(filter.namefilter) & (PokemonIV >= filter.minimumiv) & (!requestsent) ) {
                            string uri = config.PSSniperUrl+String.Format("/addpokemon/pokesniper2://{0}/{1},{2}", m.Groups["name"].Value,m.Groups["latitude"].Value,m.Groups["longtitude"].Value);
                            Console.WriteLine(String.Format("====================> Calling : {0}",uri));
                        try {
                            string result="";
                            requestsent = true; 
                            HttpClient h = new HttpClient();
                            h.Timeout = TimeSpan.FromSeconds(2);
                            result = await h.GetStringAsync(new Uri(uri));
                            requestsent = true; 
                            } catch {
                            }
                        } else {
                            //Console.WriteLine(String.Format("====================> Pokemon didn't met current  );  
                        }
                    }
                    if (!requestsent) {
                            Console.WriteLine ("Pokemon hasn't met any of filter for this channel, so skipped");
                    }

                    
                } else {
                    //Console.WriteLine(String.Format("Unable to parse the content with regexp {0}",configchannel.parseregex));
                }


                


            }
            //Console.WriteLine(String.Format("{0} {1} {2}"),message.Channel.Guild.Name,message.Channel.Name,message.Content ));
    }
}
}