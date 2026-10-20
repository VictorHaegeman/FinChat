@echo off
rem Starts one FinChat client (double-click again to open another client)
cd /d "%~dp0"
dotnet build ChatClient -v q -nologo
if errorlevel 1 (
    pause
    exit /b 1
)
start "" "ChatClient\bin\Debug\net48\ChatClient.exe"
