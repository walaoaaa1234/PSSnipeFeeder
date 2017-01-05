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
using POGOLib.Official.Extensions;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Util.Hash;
using POGOLib.Official;
using LogLevel = POGOLib.Official.Logging.LogLevel;
using System.Collections.Generic;

namespace PSSniper
{
    public class POGOLibCaller
    {

        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static PokemonInfo pokemon;
        private static Config config; 

        public static PokemonInfo VerifyPokemon(PokemonInfo Pokemonl, Config configl) 
        {
            pokemon = Pokemonl;
            config = configl;
            Run().GetAwaiter().GetResult();
            return pokemon;
        }

        private static async Task Run()
        {
            // Configure Logger
            //LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.xml"));

            //Logging.Logger.RegisterLogOutput((level, message) =>
            //{
            //   switch (level)
            //    {
            //        case LogLevel.Debug:
            //            Logger.Debug(message);
            //            break;
            //        case LogLevel.Info:
            //            Logger.Info(message);
            //            break;
            //        case LogLevel.Notice:
            //        case LogLevel.Warn:
            //            Logger.Warn(message);
            //            break;
            //        case LogLevel.Error:
            //            Logger.Error(message);
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException(nameof(level), level, null);
            //    }
            //});

            // Initiate console
            //Logger.Info("Booting up.");
            //Logger.Info("Type 'q', 'quit' or 'exit' to exit.");
            //Console.Title = "POGO Demo";

            // Configure hasher - DO THIS BEFORE ANYTHING ELSE!!
            //
            //  If you want to use the latest POGO version, you have
            //  to use the PokeHashHasher. For more information:
            //  https://talk.pogodev.org/d/51-api-hashing-service-by-pokefarmer
            //
            //  You may also not use the PokeHashHasher, it will then use
            //  the built-in hasher which was made for POGO 0.45.0. 
            //  Don't forget to use "Configuration.IgnoreHashVersion = true;" too.
            //
            //  Expect some captchas in that case..

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
            var session = await GetSession(loginProvider, latitude, longitude, true);

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

            // Retrieve the closest fort to your current player coordinates.
           //var SelectMany(c => c.CatchablePokemons)     

            var closestFort = session.Map.GetFortsSortedByDistance().FirstOrDefault();
            //session.Map
            if (closestFort != null)
            {
                IEnumerable<POGOProtos.Map.Pokemon.MapPokemon> catchable = session.Map.Cells.SelectMany(c => c.CatchablePokemons);
                
                if (catchable.Count() > 0) {
                     //POGOProtos.Map.Pokemon.MapPokemon a = catchable.ElementAt(0);
                     POGOProtos.Map.Pokemon.MapPokemon PokemonRequested = 
                        catchable.SelectMany(p=> catchable.Where(w => w.PokemonId.ToString() == pokemon.PokemonName )).FirstOrDefault();
                                                           //.Where(w => w.Latitude == pokemon.Latitude )
                                                           //.Where(w => w.Longitude == pokemon.Longtitude);
                     if  (PokemonRequested.EncounterId != 0 ) {
                         string a = "";
                         POGOProtos.Map.Pokemon.MapPokemon poketmp = PokemonRequested;
                         pokemon.EncounterId = poketmp.EncounterId;
                         pokemon.SpawnpointId = poketmp.SpawnPointId;

                     }

                }
            }
            else
            {
                Console.WriteLine("No Fort data found. Captcha soft/ip/hard ban? Check account");
            }

            //System.Threading.Thread.Sleep(5000);
            session.Shutdown();
            session.Dispose();

            // Handle quit commands.
            //HandleCommands();
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
