using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.CommandLineUtils;
namespace KestrelDemo
{
    /// <summary>
    /// Executing the "dotnet run command in the application folder will run this app.
    /// </summary>
    public class Program
    {
               
        public static void AddPokemon(string PokemonName) {
            Console.WriteLine(PokemonName);
        }

        #region snippet_Main
        public static int Main(string[] args)
        {
            CommandLineApplication cmd = new CommandLineApplication();
            //cmd.Command()
//            CommandArgument names = null;
//            cmd.Command("name", (target) =>  names = target.Argument(
//        "fullname",
//        "Enter the full name of the person to be greeted.",
 //       multipleValues: true));


            WebServer.Port = "5001";
            WebServer.NameOrIp = "localhost";
            WebServer.RunServer(args);
            return 0;
        }
        #endregion
    }
}
