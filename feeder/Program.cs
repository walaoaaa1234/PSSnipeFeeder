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
        //private static POGOLibCaller libcaller = new POGOLibCaller();
        public static dynamic Pokemons ;
        private static bool Busy = false; 

        public static Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory()+Path.DirectorySeparatorChar.ToString()+@"config.json"));

        public static void AddPokemon(dynamic Pokemon) {
           string a = JsonConvert.SerializeObject(Pokemons, Formatting.Indented);
           //Console.WriteLine("Serving pokemons: ");
           //Console.WriteLine(a);
           Console.WriteLine("==> adding pokemon");   

           if (Pokemons.Contains(Pokemon) == false) {
               if (config.verifypokemon) {
                if ( ((Pokemon.EncounterId ==0 ) | (Pokemon.SpawnpointId == null))  ) {
                    if (Busy) {
                        Console.WriteLine("====> I'm busy with checking another pokeon now. Skipping"); 
                        return; 
                    } 
                    else {
                        Busy = true; 
                        Console.WriteLine(String.Format("====> Checking/getting data (about) pokemon. It will take up to {0} seconds, please wait",config.tryforseconds.ToString()));
                        try {
                            POGOLibCaller libcaller = new POGOLibCaller();
                            Pokemon = libcaller.VerifyPokemon(Pokemon,config) ;
                        } catch {
                            Console.WriteLine(String.Format("=======> Exception happened. Search is stopped for this pokemon"));    
                        }
                        if (Pokemon.EncounterId ==0 ) {
                            Console.WriteLine(String.Format("======> Pokemon {0} not discovered at location {1} , {2} ",Pokemon.PokemonName,Pokemon.Latitude,Pokemon.Longtitude));
                        }
                    }
                }
                Busy = false; 
                if ( ((Pokemon.EncounterId ==0 ) | (Pokemon.SpawnpointId == null))  ) {
                     Console.WriteLine("======> Pokemon not added . Encounter and/or SpawnpointId are empty");
                    return;
               }

               
               }

                dynamic newlist ;
                if (config.verifypokemon) {
                    newlist = new List<PokemonInfoFull>();
                } else {
                    newlist = new List<PokemonInfo>();
                }

                foreach (dynamic poke in Pokemons) {
                    if (poke.expirationdt > DateTime.Now)  {
                    newlist.Add(poke);
                    }
                }
                Pokemons = newlist;
             
               //if ((Pokemon.EncounterId >0 ) & (Pokemon.SpawnpointId != null)) {
                    Pokemon.expirationdt = DateTime.Now.AddMinutes(config.minutestoexpire);
                    Pokemon.expiration = Convert.ToInt64((Pokemon.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
                    Pokemons.Add(Pokemon);
                    //Console.WriteLine(string.Format("Added to list: {0}:{1},{2}",Pokemon.PokemonName,Pokemon.Latitude,Pokemon.Longtitude ));
                    Console.WriteLine("======> Added: ");
                    a = JsonConvert.SerializeObject(Pokemon, Formatting.Indented);
                    Console.WriteLine(a);
               //} else {
                   //Console.WriteLine("======> Pokemon not added . Encounter and/or SpawnpointId are empty");
              // }
                      
           if (Pokemons.Count ==0 ) {
                dynamic tmppoke ;

               if (config.verifypokemon) {
                    tmppoke = new PokemonInfoFull();
               } else {
                    tmppoke = new PokemonInfo();;
               }
               
               //PokemonInfo tmppoke = new PokemonInfo();
               tmppoke.PokemonName = "Bellsprout";
               tmppoke.IV = 10;
               tmppoke.Latitude = 23.906291124969709;
               tmppoke.Longtitude = 120.59261531771575;
               if (config.verifypokemon) {
                   tmppoke.EncounterId = 13139724800732585298;
                   tmppoke.SpawnpointId = "3469343f3d9";
               }
               tmppoke.expirationdt = DateTime.Now.AddMinutes(config.minutestoexpire);
               tmppoke.expiration = Convert.ToInt64((Pokemon.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
               Pokemons.Add(tmppoke);
           }

           if (Pokemons.Count ==1 ) {
               Pokemons.Add(Pokemons[0]);
           }
           //JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
           }
        }
        #region snippet_Main
        
        static void Main(string[] args) => new Program().Run(args).GetAwaiter().GetResult();
        public async Task Run(string[] args)
        {

            dynamic tmppoke; 
            if (config.verifypokemon) {
                Pokemons = new List<PokemonInfoFull>() ;
                tmppoke = new PokemonInfoFull();
            } else {
                Pokemons = new List<PokemonInfo>() ;
                tmppoke = new PokemonInfo();
            }

            tmppoke.PokemonName = "Bellsprout";
            tmppoke.Latitude = 23.906291124969709;
            tmppoke.Longtitude = 120.59261531771575;
            if (config.verifypokemon) {
                tmppoke.EncounterId = 13139724800732585298;
                tmppoke.SpawnpointId = "3469343f3d9";
                tmppoke.IV = 10;
            }
            tmppoke.expirationdt = DateTime.Now.AddMinutes(config.minutestoexpire);
            tmppoke.expiration = Convert.ToInt64((tmppoke.expirationdt -DateTime.Parse("1/1/1970")).TotalMilliseconds);
            AddPokemon(tmppoke);

            WebServerWrapper.Port = config.webserverport; 
            WebServerWrapper.NameOrIp = config.webserveraddress;
            new WebServerWrapper().RunServer(args).GetAwaiter().GetResult();
            await Task.Delay(-1);

            //return 0;
        }
        #endregion
    }
}
