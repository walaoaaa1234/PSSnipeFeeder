#!/usr/bin/env bash
cd feeder
dotnet restore && dotnet build
cd ../DiscordCrawler
dotnet restore && dotnet build
