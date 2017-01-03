@echo off
reg import %~dp0handlers.reg
powershell -ExecutionPolicy bypass -file %~dp0scripts\PSSnipeFeeder.ps1
pause