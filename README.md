

# PSSnipeFeeder
Powershell script to feed PokemonGo-Bot with sniping information

#WARNING#
Runninng the script will change msniper:// protocol registration. If you do use  Necrobot, they will not get sniping information when you click on button on sniping websites

#- [1. Configuration](#Configuration)

If you need, change the default (5000) port in PSSnipeFeeder.ps1 

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







