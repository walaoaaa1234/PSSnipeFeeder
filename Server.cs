using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KestrelDemo
{
    /// <summary>
    /// Executing the "dotnet run command in the application folder will run this app.
    /// </summary>
    public class WebServer
    {
        public static string Server;
        public static string Port;
        public static string NameOrIp;


        #region snippet_Main
        public static int RunServer(string[] args)
        {
            //Console.WriteLine("Running demo with Kestrel.");

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var builder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory()+"/www")
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    if (config["threadCount"] != null)
                    {
                        options.ThreadCount = int.Parse(config["threadCount"]);
                    }
                })
                .UseUrls("http://"+NameOrIp+":"+Port);

            var host = builder.Build();
            Console.WriteLine("Listening at: "+Startup.Address);
            host.Start();
            do {}
            while (true);
            //return 0;
        }
        #endregion
    }
}
