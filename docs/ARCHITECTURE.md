# Architecture de FinChat

## 1. Ce qu'on reprend du squelette DistantCalculator

| Dans DistantCalculator | Dans FinChat |
|---|---|
| Projet `Communication` (bibliothèque partagée) | Même projet, avec les messages du chat |
| `interface Message` + `Expr`, `Result` `[Serializable]` | `LoginMsg`, `ChatMsg`, `PrivateMsg`, `CommandMsg`, `InfoMsg`, `UserListMsg` |
| `Net.sendMsg / rcvMsg` (BinaryFormatter) | Gardé tel quel |
| `Server` + classe `Receiver`, 1 thread par client | Même principe + liste des clients + event de diffusion |
| `ClientGuiCalculator` (Form1, Windows Forms) | Fenêtre de chat |

## 2. Vue d'ensemble

```mermaid
flowchart LR
  subgraph CA["Client Alice (Windows Forms)"]
    UA["Form1<br/>thread interface"]
    RA["thread de réception"]
  end
  subgraph CB["Client Bob (Windows Forms)"]
    UB["Form1<br/>thread interface"]
    RB["thread de réception"]
  end
  subgraph SV["Serveur (console)"]
    L["thread d'écoute<br/>AcceptTcpClient()"]
    VA["Receiver Alice<br/>1 thread"]
    VB["Receiver Bob<br/>1 thread"]
    EV{{"event BroadcastEvent<br/>(delegate)"}}
    FN["FinanceNews<br/>1 thread, 1 fois par heure"]
    LG["Logger<br/>console + server_log.txt"]
  end
  API[("API Finnhub<br/>HTTPS + clé")]

  UA -- "Net.sendMsg (TCP)" --> VA
  UB -- "Net.sendMsg (TCP)" --> VB
  L -. "new Thread(...)" .-> VA
  L -. "new Thread(...)" .-> VB
  VA -- "broadcast()" --> EV
  FN -- "broadcast()" --> EV
  EV -- "onBroadcast()" --> VA
  EV -- "onBroadcast()" --> VB
  VA -- "Net.sendMsg (TCP)" --> RA
  VB -- "Net.sendMsg (TCP)" --> RB
  RA -- "Invoke" --> UA
  RB -- "Invoke" --> UB
  FN -- "WebClient" --> API
  VA -.-> LG
  VB -.-> LG
```

## 3. Les messages (projet Communication)

```mermaid
classDiagram
  direction LR
  class Message {
    <<interface>>
    +ToString() string
  }
  class LoginMsg { -string username }
  class ChatMsg { -string sender -string channel -string text }
  class PrivateMsg { -string sender -string target -string text }
  class CommandMsg { -string command -string argument }
  class InfoMsg { -string text }
  class UserListMsg { -string[] users }
  class Net {
    +sendMsg(Stream s, Message msg)$
    +rcvMsg(Stream s)$ Message
  }
  Message <|.. LoginMsg
  Message <|.. ChatMsg
  Message <|.. PrivateMsg
  Message <|.. CommandMsg
  Message <|.. InfoMsg
  Message <|.. UserListMsg
  Net ..> Message : envoie / reçoit
```

| Classe | Sens | Utilisé pour |
|---|---|---|
| `LoginMsg` | client → serveur | Premier message après la connexion |
| `ChatMsg` | les 2 sens | Message public dans un channel (aussi les articles de FinanceBot) |
| `PrivateMsg` | les 2 sens | Message privé (`/msg bob salut`) |
| `CommandMsg` | client → serveur | `/name`, `/users`, `/join`, `/quit` |
| `InfoMsg` | serveur → client | Infos et erreurs |
| `UserListMsg` | serveur → client | Liste des connectés dans la fenêtre |

## 4. Les classes du serveur

```mermaid
classDiagram
  direction LR
  class Server {
    -int port
    -TcpListener listener
    -List~Receiver~ clients
    -static object access_grant
    +event MessageDelegate BroadcastEvent
    +start()
    +stop()
    +broadcast(Message msg)
    #onBroadcast(object sender, MessageEventArgs e)
    +sendPrivate(PrivateMsg msg, Receiver from)
    +getUsers() string[]
  }
  class Receiver {
    -TcpClient comm
    -string username
    -string channel
    +doOperation()
    +onBroadcast(object sender, MessageEventArgs e)
    +send(Message msg)
    +close(string reason)
  }
  class MessageEventArgs {
    -Message msg
    +Msg Message
  }
  class FinanceNews {
    -Server server
    +run()
  }
  class Logger {
    +log(string text)$
  }
  EventArgs <|-- MessageEventArgs
  Server "1" o-- "*" Receiver : liste des clients
  FinanceNews --> Server : broadcast()
  Receiver --> Server : broadcast() / sendPrivate()
  Server ..> MessageEventArgs
  Receiver ..> Logger
```

## 5. Un message public, étape par étape

```mermaid
sequenceDiagram
  autonumber
  participant A as Client Alice (Form1)
  participant RA as Receiver Alice
  participant S as Server
  participant RB as Receiver Bob
  participant B as Client Bob (Form1)
  A->>RA: Net.sendMsg(ChatMsg "salut")
  RA->>S: server.broadcast(msg)
  S->>S: onBroadcast() déclenche BroadcastEvent
  S-->>RA: onBroadcast(sender, e)
  S-->>RB: onBroadcast(sender, e)
  Note over RA,RB: chaque Receiver garde le message seulement si c'est son channel
  RA->>A: Net.sendMsg(ChatMsg)
  RB->>B: Net.sendMsg(ChatMsg)
  B->>B: thread de réception, puis Invoke vers la TextBox
```

## 6. Le channel #finance

```mermaid
sequenceDiagram
  participant F as FinanceNews (thread serveur)
  participant API as API Finnhub
  participant S as Server
  participant C as Clients dans #finance
  F->>F: lit la clé dans finnhub_key.txt
  loop toutes les heures
    F->>API: WebClient.DownloadString(url + clé)
    API-->>F: JSON (liste d'articles)
    F->>F: DataContractJsonSerializer, prend le 1er article
    F->>S: broadcast(ChatMsg "FinanceBot", "finance", titre + lien)
    S-->>C: BroadcastEvent (les clients de #general l'ignorent)
    F->>F: Thread.Sleep(3 600 000 ms)
  end
```

## 7. Les threads et ce qu'on protège

| Programme | Thread | Rôle |
|---|---|---|
| Serveur | principal | Lit `start` / `stop` / `exit` dans la console |
| Serveur | écoute | Boucle sur `AcceptTcpClient()`, crée un `Receiver` + son thread |
| Serveur | 1 par client | `Receiver.doOperation()` : lit les messages du client |
| Serveur | finance | `FinanceNews.run()` : 1 article par heure |
| Client | interface | Fenêtre, boutons, envoi |
| Client | réception | `Form1.receive()` : `Net.rcvMsg` puis `Invoke` |

| Donnée partagée | Problème | Protection |
|---|---|---|
| Liste des clients | Un client arrive pendant qu'on parcourt la liste | `lock(access_grant)`, objet `static` |
| Le flux d'un client | Deux threads écrivent en même temps dans le même socket | `lock(send_grant)` dans `Receiver.send()` |
| Fichier de log | Deux lignes écrites en même temps | `lock` dans `Logger.log()` |
| TextBox du client | `InvalidOperationException` (cross-thread) | `InvokeRequired` + `Invoke` |
