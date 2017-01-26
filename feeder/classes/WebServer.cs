using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Globalization;
namespace PSSniper
{
    public class WebServerStartup
    {
        public static string Address;
        public WebServerStartup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
        public IConfigurationRoot Configuration { get; private set; }

        #region snippet_Configure
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            Address =  serverAddressesFeature.Addresses.ElementAt(0);
            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                string path = context.Request.Path;
                //Console.WriteLine("request received : "+path);

                if (path == "/") {
                    context.Response.ContentType = "application/json";
                    string a = JsonConvert.SerializeObject(Program.Pokemons, Formatting.Indented);
                    await context.Response.WriteAsync(a);
                }
       
                if (path.StartsWith("/addpokemon")) {
                    Console.WriteLine("request received : "+path);
                    dynamic Pokemon;
                    if (Program.config.verifypokemon) {
                        Pokemon = new PokemonInfoFull();
                    }else {
                        Pokemon = new PokemonInfo();
                    }
                    path = path.Replace(System.Environment.NewLine, "");
                    string[] strings = Regex.Split(path,"/");
                if (strings[2]== "msniper:") {
                    //Console.WriteLine("processing msniper");   
                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    Pokemon.PokemonName = strings[4];
                    if (Program.config.verifypokemon) {
                        Pokemon.EncounterId = Convert.ToUInt64(strings[5]);
                        Pokemon.SpawnpointId = strings[6];
                        Pokemon.IV =  Convert.ToDouble(strings[8],culture);
                    }
                    Pokemon.Latitude = Convert.ToDouble(Regex.Split(strings[7],",")[0],culture);
                    Pokemon.Longtitude = Convert.ToDouble(Regex.Split(strings[7],",")[1],culture);
                    //Console.WriteLine("msniper done");   
                }
                if (strings[2]=="pokesniper2:") {
                    //Console.WriteLine("processing pokesniper2");   
                    Pokemon.PokemonName = strings[4];
                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    Pokemon.Latitude = Convert.ToDouble(Regex.Split(strings[5],",")[0],culture);
                    Pokemon.Longtitude = Convert.ToDouble(Regex.Split(strings[5],",")[1],culture);
                    if (strings.Count()==7) {
                        Pokemon.IV = Convert.ToDouble(strings[6],culture); 
                    }
                    //Console.WriteLine("pokesniper2 done");   
                }
                if (Pokemon.PokemonName !=null) {
                   Program.AddPokemon(Pokemon);
                }
            }
                
                if (path == "/registerDISABLED") {
                    context.Response.ContentType = "text/html";
                    string content=$@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"">
<head>
<title>Web Protocol Handler Sample - Register</title>
<script type=""text/javascript"">
window.navigator.registerProtocolHandler(
    ""msniper3"",
    """+Address+"/addpokemon/%s"+@""",
    'PSSniper');
</script>
</head>
</html>";
                    await context.Response.WriteAsync(content);
                } 
                if (path == "/test") {
                    context.Response.ContentType = "text/html";
                    string content =@"<a href=""msniper3://aaaa/bbbbb/cccc"">click</a>";
                    await context.Response.WriteAsync(content);
                }
            });
        }
        #endregion
    }
}
