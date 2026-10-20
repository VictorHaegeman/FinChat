@echo off
rem Starts the FinChat server (run it from this folder so that finnhub_key.txt is found)
cd /d "%~dp0"
dotnet run --project ChatServer
pause
