@echo off
reg import %~dp0handlers.reg
powershell -ExecutionPolicy bypass -file %~dp0PSSnipeFeeder.ps1