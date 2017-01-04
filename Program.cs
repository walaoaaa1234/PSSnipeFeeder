using System;
using System.Collections.Generic;
namespace PSSniper
{
    /// <summary>
    /// Executing the "dotnet run command in the application folder will run this app.
    /// </summary>
    public class Program
    {
        public static List<PokemonInfo> Pokemons = new List<PokemonInfo>() ;
        public static void AddPokemon(PokemonInfo Pokemon) {
           Pokemons.Add(Pokemon) ;
        }
        #region snippet_Main
        public static int Main(string[] args)
        {
            WebServerWrapper.Port = "5001";
            WebServerWrapper.NameOrIp = "127.0.0.1";
            WebServerWrapper.RunServer(args);
            
            
            
            return 0;
        }
        #endregion
    }
}
