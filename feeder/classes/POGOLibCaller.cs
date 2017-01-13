using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
//using NLog;
//using NLog.Config;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOProtos.Data;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Util.Hash;
using POGOLib.Official;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PSSniper
{
    public class POGOLibCaller
    {

        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Session session;
        public static long cooldown ; 
        private static PokemonInfoFull pokemon;
        private static Config config; 
        //private static int UpdateCounter=0;

        public  PokemonInfoFull VerifyPokemon(PokemonInfoFull Pokemonl, Config configl) 
        {
            pokemon = Pokemonl;
            config = configl;
            Run().GetAwaiter().GetResult();
            return pokemon;
        }

        private static async Task Run()
        {
            if ( cooldown >  POGOLib.Official.Util.TimeUtil.GetCurrentTimestampInMilliseconds()) {  
                 Console.WriteLine("======> cool down");
                 return; 
            }
           
            var pokeHashAuthKey = config.hashkey;

            Configuration.Hasher = new PokeHashHasher(pokeHashAuthKey);
            // Configuration.IgnoreHashVersion = true;

            // Settings
            var loginProviderStr = config.auth_service;
            var usernameStr = config.username;
            var passwordStr = config.password;

            // Login
            ILoginProvider loginProvider;

            switch (loginProviderStr)
            {
                case "google":
                    loginProvider = new GoogleLoginProvider(usernameStr, passwordStr);
                    break;
                case "ptc":
                    loginProvider = new PtcLoginProvider(usernameStr, passwordStr);
                    break;
                default:
                    throw new ArgumentException("Login provider must be either \"google\" or \"ptc\".");
            }

            var locRandom = new Random();
            var latitude = pokemon.Latitude;
            var longitude = pokemon.Longtitude;
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            double start_latitude = Convert.ToDouble(Regex.Split(config.startlocation,",")[0],culture);
            double start_longtitude = Convert.ToDouble(Regex.Split(config.startlocation,",")[1],culture);
            if (session == null) {
                //Console.WriteLine("==>Creating session/logging in. ");
                session = await GetSession(loginProvider, start_latitude, start_longtitude, true);
                SaveAccessToken(session.AccessToken);
                session.AccessTokenUpdated += SessionOnAccessTokenUpdated;
                //session.Player.Inventory.Update += InventoryOnUpdate;
                session.Map.Update += MapOnUpdate;

                // Send initial requests and start HeartbeatDispatcher.
                // This makes sure that the initial heartbeat request finishes and the "session.Map.Cells" contains stuff.
                if(!await session.StartupAsync())
                {
                    throw new Exception("Session couldn't start up.");
                }
            }
            //Console.WriteLine("==>Teleporting to pokemon");
            session.Player.SetCoordinates (latitude,longitude);
            var closestFort = session.Map.GetFortsSortedByDistance().FirstOrDefault();
            int i=1; 
            do {
                Console.Write(String.Format("\rPokemon {0} attemp {1} of {2}",pokemon.PokemonName, i.ToString(),config.tryforseconds.ToString()));
                await session.RpcClient.RefreshMapObjectsAsync();
                // Retrieve the closest fort to your current player coordinates.
                closestFort = session.Map.GetFortsSortedByDistance().FirstOrDefault();
                if (closestFort != null)
                {
                        cooldown = closestFort.CooldownCompleteTimestampMs; 
                        IEnumerable<POGOProtos.Map.Pokemon.MapPokemon> catchable = session.Map.Cells.SelectMany(c => c.CatchablePokemons);
                        SearchForPokemon(catchable).GetAwaiter().GetResult();
                        /*if (pokemon.EncounterId == 0) {
                            int i=1;
                            do {
                                //Console.WriteLine ("repeating: "+i.ToString());
                                await session.RpcClient.RefreshMapObjectsAsync();
                                //System.Threading.Thread.Sleep(1000);
                                catchable = session.Map.Cells.SelectMany(c => c.CatchablePokemons);
                                SearchForPokemon(catchable).GetAwaiter().GetResult();
                                i++;
                            } while    (i<60 & (pokemon.EncounterId==0) );
                            //} while    (pokemon.EncounterId==0 );
                        }*/
                } 
                i++;
            } while  (i<=config.tryforseconds & (pokemon.EncounterId==0) ); 
            if (closestFort == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("======> No Fort data found. Captcha soft/ip/hard ban? Check account, take a rest , etc. ");
                Console.ForegroundColor = ConsoleColor.White; 
                session.Player.SetCoordinates (start_latitude,start_longtitude);
                //session.Shutdown();
                //session.Dispose();
                //session = null;
            }
            //Console.WriteLine("Teleporting back to startup");
            session.Player.SetCoordinates (start_latitude,start_longtitude);
            //System.Threading.Thread.Sleep(5000);
            //session.Shutdown();
            //session.Dispose();

            // Handle quit commands.
            //HandleCommands();
        }
        private static async Task SearchForPokemon ( IEnumerable<POGOProtos.Map.Pokemon.MapPokemon> catchable) {

                if (catchable.Count() > 0) {
                     //POGOProtos.Map.Pokemon.MapPokemon a = catchable.ElementAt(0);
                     POGOProtos.Map.Pokemon.MapPokemon PokemonRequested = 
                        catchable.SelectMany(p=> catchable.Where(w => w.PokemonId.ToString() == pokemon.PokemonName )).FirstOrDefault();
                                                           //.Where(w => w.Latitude == pokemon.Latitude )
                                                           //.Where(w => w.Longitude == pokemon.Longtitude);
                     if  (PokemonRequested != null ) {
                            //string a = "";
                            try {
                                var responseDetailsBytes = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                                {
                                    RequestType = RequestType.Encounter,
                                    RequestMessage = new EncounterMessage
                                    {
                                        EncounterId  = PokemonRequested.EncounterId,
                                        SpawnPointId = PokemonRequested.SpawnPointId,
                                        PlayerLatitude = PokemonRequested.Latitude,
                                        PlayerLongitude = PokemonRequested.Longitude
                                    }.ToByteString()
                                });
                                EncounterResponse DetailsResponse = EncounterResponse.Parser.ParseFrom(responseDetailsBytes);
                                PokemonData pokemondata = DetailsResponse.WildPokemon.PokemonData;
                                int IvSum = pokemondata.IndividualAttack + pokemondata.IndividualDefense + pokemondata.IndividualStamina;

                                pokemon.IV = System.Math.Round(((double)IvSum/45),2)*100;
                                pokemon.CP = pokemondata.Cp;
                                pokemon.Move1 = pokemondata.Move1.ToString();
                                pokemon.Move2 = pokemondata.Move2.ToString();
                                
                            } catch {

                            };
                            pokemon.EncounterId = PokemonRequested.EncounterId;
                            pokemon.SpawnpointId = PokemonRequested.SpawnPointId;
                            
                     } else {
                         string b="";
                     }

                }


        }
        private static void SessionOnAccessTokenUpdated(object sender, EventArgs eventArgs)
        {
            var session = (Session)sender;

            SaveAccessToken(session.AccessToken);

            //Logger.Info("Saved access token to file.");
        }

        private static void InventoryOnUpdate(object sender, EventArgs eventArgs)
        {
            //Logger.Info("Inventory was updated.");
        }

        private static void MapOnUpdate(object sender, EventArgs eventArgs)
        {
            //string a;
            //Logger.Info("Map was updated.");
            //Console.WriteLine("map updated");
            //UpdateCounter++;
        }

        private static void SaveAccessToken(AccessToken accessToken)
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Cache", $"{accessToken.Uid}.json");

            File.WriteAllText(fileName, JsonConvert.SerializeObject(accessToken, Formatting.Indented));
        }

        private static void HandleCommands()
        {
            var keepRunning = true;

            while (keepRunning)
            {
                var command = Console.ReadLine();

                switch (command)
                {
                    case "q":
                    case "quit":
                    case "exit":
                        keepRunning = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Login to PokémonGo and return an authenticated <see cref="Session" />.
        /// </summary>
        /// <param name="loginProvider">Provider ID must be 'PTC' or 'Google'.</param>
        /// <param name="initLat">The initial latitude.</param>
        /// <param name="initLong">The initial longitude.</param>
        /// <param name="mayCache">Can we cache the <see cref="AccessToken" /> to a local file?</param>
        private static async Task<Session> GetSession(ILoginProvider loginProvider, double initLat, double initLong, bool mayCache = false)
        {
            var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Cache");
            var fileName = Path.Combine(cacheDir, $"{loginProvider.UserId}-{loginProvider.ProviderId}.json");

            if (mayCache)
            {
                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                if (File.Exists(fileName))
                {
                    var accessToken = JsonConvert.DeserializeObject<AccessToken>(File.ReadAllText(fileName));

                    if (!accessToken.IsExpired)
                        return Login.GetSession(loginProvider, accessToken, initLat, initLong);
                }
            }

            var session = await Login.GetSession(loginProvider, initLat, initLong);

            if (mayCache)
                SaveAccessToken(session.AccessToken);

            return session;
        }

    }
}
