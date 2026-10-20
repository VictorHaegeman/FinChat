# Guide du code FinChat

Ce fichier sert à retrouver vite n'importe quoi dans le code.

**Astuce Cursor :** `Ctrl+Shift+F` puis tape un tag (par exemple `COURS 3`) pour voir tous les endroits du code qui utilisent ce chapitre. `Ctrl+P` puis le nom d'un fichier pour l'ouvrir.

---

## 1. Les fichiers

| Fichier | Projet | Rôle |
|---|---|---|
| `Communication/Message.cs` | Communication | Les 6 types de messages + l'interface `Message` |
| `Communication/Net.cs` | Communication | `sendMsg` / `rcvMsg` : envoyer / recevoir un message (repris du squelette) |
| `ChatServer/Program.cs` | Serveur | Point d'entrée, commandes console `start` / `stop` / `exit` |
| `ChatServer/Server.cs` | Serveur | Écoute, liste des clients, **event de diffusion**, messages privés |
| `ChatServer/Receiver.cs` | Serveur | Un objet + un thread par client : lit ses messages et agit |
| `ChatServer/MessageEventArgs.cs` | Serveur | Le delegate `MessageDelegate` + la classe `MessageEventArgs` |
| `ChatServer/FinanceNews.cs` | Serveur | Thread qui envoie un article financier toutes les heures dans #finance |
| `ChatServer/Logger.cs` | Serveur | Logs dans la console + `server_log.txt` |
| `ChatClient/Program.cs` | Client | Point d'entrée, ouvre la fenêtre |
| `ChatClient/Form1.cs` | Client | Logique de la fenêtre : connexion, envoi, réception, affichage |
| `ChatClient/Form1.Designer.cs` | Client | Les contrôles de la fenêtre (positions, couleurs) |

Autres fichiers à la racine : `start-server.bat` et `start-client.bat` (lancement en double-clic), `finnhub_key.example.txt` (modèle du fichier de clé), `.gitignore` (ce qui ne va pas sur GitHub : `bin/`, `obj/`, `finnhub_key.txt`, `server_log.txt`).

Autres documents : `ARCHITECTURE.md` (schémas), `COURS.md` (le cours relié au code + questions d'oral, gardé sur le PC, pas sur GitHub), `JUSTIFICATIONS.md` (chaque choix du code justifié par une slide, pour l'oral, gardé sur le PC aussi), `TESTS.md` (plan de test), `RAPPORT.md` (doc à rendre).

Chaque fichier commence par un en-tête : son rôle, les chapitres du cours utilisés, et le plan du fichier (`#region`). Dans Cursor, les `#region` se replient avec la petite flèche dans la marge.

---

## 2. Je cherche... → je vais où

| Fonction | Où | Méthode |
|---|---|---|
| Démarrer / arrêter le serveur | `Server.cs` | `start()`, `stop()` |
| Accepter un client | `Server.cs` | `listen()` |
| Lire les messages d'un client | `Receiver.cs` | `doOperation()` |
| Login (pseudo) | `Receiver.cs` | `login()` |
| Pseudo déjà pris | `Server.cs` | `reserveName()` |
| Message public | `Receiver.cs` → `Server.cs` | `publicMessage()` → `broadcast()` |
| **Diffusion avec l'event** | `Server.cs` | `onBroadcast()` + `event BroadcastEvent` |
| Qui reçoit la diffusion | `Receiver.cs` | `onBroadcast()` (filtre par channel) |
| Abonnement `+=` / désabonnement `-=` | `Receiver.cs` | `login()` / `close()` |
| Message privé | `Server.cs` | `sendPrivate()` |
| Commandes `/name /users /join /quit` | `Receiver.cs` | `command()` |
| Commande `/help` (répondue par le client) | `Form1.cs` | `sendCommand()` |
| Lecture de la clé API | `FinanceNews.cs` | `readKey()` |
| Liste des connectés | `Server.cs` | `getUsers()`, `sendUserList()` |
| Article finance | `FinanceNews.cs` | `run()`, `sendLastArticle()` |
| Logs | `Logger.cs` | `log()` |
| Bouton Connect | `Form1.cs` | `buttonConnect_Click()` |
| Envoi d'un message / d'une commande | `Form1.cs` | `buttonSend_Click()`, `sendCommand()` |
| Thread de réception du client | `Form1.cs` | `receive()` |
| Affichage thread-safe (`Invoke`) | `Form1.cs` | `display()`, `disconnected()` |

---

## 3. Le cours → où dans le code

Chaque ligne importante porte un tag en commentaire. Cherche le tag avec `Ctrl+Shift+F`.

| Tag à chercher | Notion | Où principalement |
|---|---|---|
| `COURS 1 - Basics` | classes, attributs privés, propriétés, `static` | `Message.cs`, `Logger.cs` |
| `COURS 2 - Classification` | interface, polymorphisme, `is` + cast | `Message.cs`, `Receiver.doOperation()`, `Form1.display()` |
| `COURS 2 - Exceptions` | `try` / `catch` | `Receiver.doOperation()`, `FinanceNews` |
| `COURS 3 - Delegates` | delegate, event, `+=` / `-=`, `onXxx` | `MessageEventArgs.cs`, `Server.onBroadcast()`, `Receiver.onBroadcast()` |
| `COURS 4 - Threads` | `Thread`, `ThreadStart`, `Sleep`, `Abort` | `Server.listen()`, `Receiver.doOperation()`, `FinanceNews`, `Form1.receive()` |
| `COURS 4 - Network` | `TcpListener`, `TcpClient`, `GetStream()` | `Server.start()`, `Server.listen()`, `Form1.buttonConnect_Click()` |
| `COURS 5 - IO` | `StreamWriter`, `StreamReader` | `Logger.log()`, `FinanceNews.readKey()` |
| `COURS 6 - Serialization` | `[Serializable]`, `BinaryFormatter` | `Message.cs`, `Net.cs` |
| `COURS 8 - Collections` | `List<T>` | `Server.clients` |
| `LAB 2 - Invoke` | `InvokeRequired` + `Invoke` | `Form1.display()`, `Form1.disconnected()` |
| `LAB 4 - lock` | `lock(access_grant)` | `Server.cs` (liste des clients), `Receiver.send()`, `Logger.log()` |
| `HORS COURS` | requête HTTP + JSON | `FinanceNews.getLastArticle()` uniquement |

---

## 4. Le trajet d'un message public

1. **Client** `Form1.buttonSend_Click()` → `send(new ChatMsg(...))` → `Net.sendMsg()`
2. **Serveur** `Receiver.doOperation()` reçoit avec `Net.rcvMsg()`, voit `msg is ChatMsg`
3. `Receiver.publicMessage()` → `server.broadcast(...)`
4. `Server.onBroadcast()` déclenche `BroadcastEvent`
5. Le delegate appelle `Receiver.onBroadcast()` de **chaque** client abonné
6. Chaque `Receiver` garde le message si c'est son channel → `send()` → `Net.sendMsg()`
7. **Client** `Form1.receive()` (thread de réception) reçoit → `display()` → `Invoke` → la TextBox

---

## 5. Les threads

| Programme | Thread | Méthode |
|---|---|---|
| Serveur | principal | `Program.Main()` : lit `start` / `stop` / `exit` |
| Serveur | écoute | `Server.listen()` |
| Serveur | 1 par client | `Receiver.doOperation()` |
| Serveur | finance | `FinanceNews.run()` |
| Client | interface | la fenêtre (boutons, envoi) |
| Client | réception | `Form1.receive()` |
