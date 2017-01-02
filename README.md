# PSSnipeFeeder
Powershell script to feed PokemonGo-Bot with sniping information

#WARNING#
Runninng the script will change msniper:// and pokesniper2:// protocols registration. If you do use PokeSniper software or Necrobot, they will not get sniping information when you click on button on sniping websites

- [1. Configuration](#Configuration)

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
						"expiration": { "param": "expiration", "format": "milliseconds" }
					}
				},
```


- [2. Usage](#Usage)
Run run_me.bat .

Then open any website which has buttons for PokeSnipe or Msniper. Click on the button to snipe the pokemon that you want.
Additionaly in edit text editor you can prepare information in format like below, to ask your bot to snipe the pokemon.
Just prepare a text in format
pokesniper2://PokemonName/Coordinates

for example:
pokesniper2://Dratini/25.073503,121.618995

then, just, copy the line to clipboard. The clipboard is monitored by script and, then, information is passed to the bot.

Please be informed , that during startup of the bot you should have at least two pokemons added (just click on PokeSniper button on two pokemons, for example on Http://msniper.com )  . Otherwise your bot will say, that response from http://127.0.0.1:5000 (when default port is used) is incorrect and bot will ignore that feed. 

Enjoy!







