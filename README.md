# PSSnipeFeeder
.Net core binary to feed PokemonGo-Bot (any bot which can read json files) with sniping information
It can on any system (Windows/Linux/MacOS) ! However the easier way to provide information about Pokemons is on Windows (read below)

# Installation and update
1. Install .net Core: https://www.microsoft.com/net/download/core#/current
2. Run commands: 
```
git clone https://github.com/pogarek/PSSnipeFeeder 
dotnet restore
```

##Update
```
git pull
dotnet restore
```

# Configuration



Then add below to Sources in Sniper task configuration in your PokemonGo-Bot configuration:
```
  {
				  "enabled": true,
					"url": "http://127.0.0.1:5000/",
					"timeout": 3,
					"mappings": {
						"name": { "param": "name" },
						"latitude": { "param": "latitude" },
						"longitude": { "param": "longitude" },
			            "spawnpoint": { "param": "spawnpoint" },
						"encounter": { "param": "encounter" },
						"expiration": { "param": "expiration", "format": "milliseconds" }
					}
				},
```


#- [2. Usage](#Usage)
Run run_me.bat .

Then open any website which has buttons for Msniper. Click on the button to snipe the pokemon that you want.
Additionaly in edit text editor you can prepare information in format like below, to ask your bot to snipe the pokemon.
Just prepare a text in format
msniper://PokemonName/encounterid/spawnpointid/latitude,longitude/iv  

for example:
msniper://Beedrill/13771894552293369407/3403ff2f5e7/22.330832561292816,114.10366351376578/59.64  

then, just, copy the line to clipboard. The clipboard is monitored by script and, then, information is passed to the bot.

Regardless if pokemon was added manually or by pressing the button : it will be added to the bot for period of 3 minutes. 

Please be informed , that during startup of the bot you should have at least two pokemons added (just click on MSniper button on two pokemons, for example on Http://msniper.com )  . Otherwise your bot will say, that response from http://127.0.0.1:5000 (when default port is used) is incorrect and bot will ignore that feed. 

Enjoy!







