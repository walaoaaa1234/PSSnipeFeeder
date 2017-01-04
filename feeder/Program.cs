using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
namespace PSSniper
{
    /// <summary>
    /// Executing the "dotnet run command in the application folder will run this app.
    /// </summary>
    public class Program
    {
        public static List<PokemonInfo> Pokemons = new List<PokemonInfo>() ;

        private static Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        public static void AddPokemon(PokemonInfo Pokemon) {
           foreach (PokemonInfo poke in Pokemons) {
               if (poke.expirationdt < DateTime.Now)  {
                   Pokemons.Remove(poke);
               }
           }
           if (Pokemons.Contains(Pokemon) == false) {
               if ((Pokemon.EncounterId ==0 ) | (Pokemon.SpawnpointId == null)) {
                   Pokemon = POGOLibCaller.VerifyPokemon(Pokemon,config) ;
               }
               if ((Pokemon.EncounterId >0 ) & (Pokemon.SpawnpointId != null)) {
                    Pokemon.expirationdt = DateTime.Now.AddMinutes(3);
                    Pokemon.expiration = Convert.ToInt64((Pokemon.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
                    Pokemons.Add(Pokemon);
                     Console.WriteLine(string.Format("Added to list: {0}:{1},{2}",Pokemon.PokemonName,Pokemon.Latitude,Pokemon.Longtitude ));
           }
                      
           if (Pokemons.Count ==0 ) {
               PokemonInfo tmppoke = new PokemonInfo();
               tmppoke.PokemonName = "Bellsprout";
               tmppoke.IV = 10;
               tmppoke.Latitude = 23.906291124969709;
               tmppoke.Longtitude = 120.59261531771575;
               tmppoke.EncounterId = 13139724800732585298;
               tmppoke.SpawnpointId = "3469343f3d9";
               tmppoke.expirationdt = DateTime.Now.AddMinutes(3);
               tmppoke.expiration = Convert.ToInt64((Pokemon.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
               Pokemons.Add(tmppoke);
           }

           if (Pokemons.Count ==1 ) {
               Pokemons.Add(Pokemons[0]);
           }
           //JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
           string a = JsonConvert.SerializeObject(Pokemons, Formatting.Indented);
           Console.WriteLine("Serving pokemons: ");
           Console.WriteLine(a);
           }
        }
        #region snippet_Main
        
        public static int Main(string[] args)
        {
            PokemonInfo tmppoke = new PokemonInfo();
            tmppoke.PokemonName = "Bellsprout";
            tmppoke.IV = 10;
            tmppoke.Latitude = 23.906291124969709;
            tmppoke.Longtitude = 120.59261531771575;
            tmppoke.EncounterId = 13139724800732585298;
            tmppoke.SpawnpointId = "3469343f3d9";
            tmppoke.expirationdt = DateTime.Now.AddMinutes(3);
            tmppoke.expiration = Convert.ToInt64((tmppoke.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
            AddPokemon(tmppoke);
            //Pokemons.Add(tmppoke);
            

            WebServerWrapper.Port = config.webserverport; 
            WebServerWrapper.NameOrIp = config.webserveraddress;
            WebServerWrapper.RunServer(args);
            
            return 0;
        }
        #endregion
    }
}
