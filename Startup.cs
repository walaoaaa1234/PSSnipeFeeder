using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace KestrelDemo
{
    public class Startup
    {
        public static string Address;
        public Startup(IHostingEnvironment env)
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

                if (path == "/") {
                    string jsonpath = Directory.GetCurrentDirectory()+@"\www\poke.json";
                    string  a =System.IO.File.ReadAllText(jsonpath);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(a);
                }
                if (path.StartsWith("/addpokemon")) {
                    string[] tmp = Regex.Split(path, "/");
                    string PokemonName = tmp[2];
                    Program.AddPokemon(PokemonName);
                }
                if (path == "/register") {
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
