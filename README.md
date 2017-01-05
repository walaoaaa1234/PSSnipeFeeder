``
# PSSnipeFeeder
.Net core binary to feed PokemonGo-Bot (any bot which can read json files) with sniping information
It can on any system (**Windows/Linux/MacOS**) ! 
*However on Windows you can click on links on sniping website to feed your bot (read below)*

# Installation and update
1. Install .net Core: https://www.microsoft.com/net/download/core#/current/sdk (SDK)
2. Run commands: 
```
git clone https://github.com/pogarek/PSSnipeFeeder
cd PSSnipeFeeder
dotnet restore
```

##Update
```
git pull
dotnet restore
```

# Configuration
1. Copy feeder\config.json.example to feeder\config.json and edit it. I recommend using a dedicated (with 1st pokemon caught etc) account for the tool. 
2. Update your bot configuration so it can call your url (http://127.0.0.1:5000/ by default)
For example for PokemonGo-Bot add below to Sniper task
```
 				{
				  "enabled": true,
					"url": "http://127.0.0.1:5000/",
					"timeout": 3,
					"mappings": {
           			"iv": { "param": "IV" },
					"name": { "param": "PokemonName" },
					"latitude": { "param": "Latitude" },
					"longitude": { "param": "Longtitude" },
            		"spawnpoint": { "param": "SpawnpointId" },
					"encounter": { "param": "EncounterId" },
					"expiration": { "param": "expiration", "format": "milliseconds" }
					}
				}
```


#Usage

````
cd feeder
dotnet run
```

Open a URL (in browser for example) with address:

## using msniper format

```
http://webserveraddress:webserverport/addpokemon/msniper://PokemonName/encounterid/spawnpointid/latitude,longitude/iv  
```

for example:
```
http://127.0.0.1:5000/addpokemon/msniper://Beedrill/13771894552293369407/3403ff2f5e7/22.330832561292816,114.10366351376578/59.64  
```

##using pokesniper2 format: 

```
http://webserveraddress:webserverport/addpokemon/pokesniper2://PokemonName/latitude,longitude
```

for example:
```
http://127.0.0.1:5000/addpokemon/pokesniper2://Beedrill/22.330832561292816,114.10366351376578
```

For Pokesnipers2 this software will connect to Pokemons server (sorry, hashkey required)  to gather EncounterId and SpawnLocationsID and to check, if pokemon is there. 

##Windows Only (unless you know how to register protocol handlers msniper:// or pokesniper2:// or another operating system - then let me know by creating an Issue on GitHub)

You can pass information directly to bot from sniping websites, like http://msniper.com or http://pokezzz.com. 
Just run Windows\runme.bat to register the functionality, then you can click on sniping websites on Msniper or PokeSniper2 buttons. 

Regardless if pokemon was added manually or by pressing the button : it will be added to the bot for period of 3 minutes. 

Enjoy!







