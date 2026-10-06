# FinChat

Client–server chat in C# (.NET Framework 4.8), built from the DistantCalculator skeleton.

## Run

1. Put your Finnhub key in `ChatServer/FinanceNews.cs` (`API_KEY`).
2. Server (one terminal):

       dotnet run --project ChatServer

   Console commands: `start`, `stop`, `exit`.
3. Clients (one terminal per client):

       dotnet run --project ChatClient

## Chat commands

- `/msg user text` : private message
- `/name newname` : change username
- `/users` : list connected users
- `/join general` or `/join finance` : change channel
- `/quit` : leave the chat
