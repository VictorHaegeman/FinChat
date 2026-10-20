# Le cours relié au code

Pour chaque chapitre du cours : l'idée à retenir, où elle est dans FinChat, et les questions probables à l'oral avec une réponse courte.

Pour voir tous les endroits d'un chapitre dans le code : `Ctrl+Shift+F` dans Cursor, puis le tag (par exemple `COURS 3`).

> Pour la justification de **chaque choix** du code (slide par slide, avec l'alternative écartée), voir `JUSTIFICATIONS.md`.

---

## Chapitre 1 · C# basics — tag `COURS 1`

**À retenir (slides)**
- Tout le code est dans des **classes**. Les objets sont des **références**, créés avec `new`, libérés par le garbage collector (p5).
- **Constructeurs** : même nom que la classe ; un constructeur peut en appeler un autre avec `: this(...)` (p7-8).
- **`static`** : attribut ou méthode **de classe**, partagé par tous les objets. Une méthode `static` ne peut utiliser que des membres `static`, et s'appelle avec le nom de la classe (p9-10).
- **Accès** : `public`, `private`, `protected`. **`private` est la valeur par défaut** des membres (p11).
- **Tableaux** : `System.Array`, indice à partir de 0, taille fixée avant usage ; `foreach` pour les parcourir (p12-21).
- **Propriétés** (p23-28) : `get` (sans parenthèses, renvoie le type de la propriété) et `set` (utilise `value`). Elles peuvent contenir du code pour un **accès sûr** (exemple : `Count` limité à 0..100).
- **Encapsulation** (p31-33) : dans sa propre classe, on peut toucher l'attribut `private` (« no privacy ») ; depuis une autre classe, il n'est « même pas proposé » : il faut passer par la propriété.
- Surcharge d'opérateurs et indexeurs `this[int i]` (p34-41) : pas utilisés dans FinChat.
- **.NET** : le code est compilé en MSIL dans une **assembly** (`.exe`), exécutée par le CLR. `csc /t:library` crée une **`.dll`** (p42-47).

**Dans FinChat**
- `Message.cs` : attributs `private`, propriétés en lecture seule (`get` uniquement), valeurs données dans le constructeur.
- `Logger.log()` est `static` : on l'appelle `Logger.log("...")`, et son verrou `access_grant` est donc `static` lui aussi.
- `Communication` est une **DLL** utilisée par le serveur et le client.
- Constantes : `const string fileName`, `KEY_FILE`, `URL`, `INTERVAL`.

**Questions possibles**
- *Pourquoi les attributs sont `private` ?* Seule la classe peut modifier ses données (p11, p33). Ex. : la liste des clients n'est modifiée que dans `Server`, sous `lock`.
- *Pourquoi les messages n'ont pas de `set` ?* Un message ne change plus une fois créé. Le serveur en recrée un avec le vrai pseudo : un client ne peut pas se faire passer pour un autre.
- *Pourquoi `access_grant` est `static` ?* Pour que tous les threads verrouillent le même objet (Ch4 p20).
- *Pourquoi une DLL `Communication` ?* Le client et le serveur doivent avoir exactement les mêmes classes pour que la désérialisation marche.

---

## Chapitre 2 · Classification (héritage, polymorphisme, interfaces, exceptions) — tags `COURS 2`

**À retenir**
- Une **interface** est un contrat : des signatures sans code, pas d'attributs. Une classe peut implémenter **plusieurs** interfaces mais n'hériter que d'**une** classe.
- **Polymorphisme** : une variable de type interface (ou classe mère) peut contenir un objet d'une classe fille.
- On retrouve le vrai type avec **`is`**, puis on fait un **cast** : `if (a is snake) ((snake)a).doSomething();`
- `virtual` / `override` = **late binding** : la méthode appelée dépend du type réel de l'objet. Sans `override`, c'est du **masquage** (`new`).
- **Exceptions** : `try` / `catch` / `finally`. Une exception non attrapée arrête le programme. Le premier `catch` compatible est utilisé.

**Dans FinChat**
- `interface Message` : `LoginMsg`, `ChatMsg`… l'implémentent tous. `Net.rcvMsg()` renvoie un `Message`, sans savoir lequel.
- `Receiver.doOperation()` : `if (msg is ChatMsg) publicMessage((ChatMsg)msg);` exactement le modèle de la slide « checking class ».
- `ToString()` est redéfini (`override`) dans chaque message : le client affiche `msg.ToString()` sans connaître le type.
- `MessageEventArgs : EventArgs` : héritage.
- `try` / `catch` : client qui se déconnecte brutalement (`Receiver.doOperation()`), API qui ne répond pas (`FinanceNews`).

**Questions possibles**
- *Pourquoi une interface `Message` ?* Pour que `Net.sendMsg` et `rcvMsg` acceptent **n'importe quel** type de message. C'est le choix du squelette du prof.
- *Comment le serveur sait quel message il a reçu ?* Avec `is`, puis un cast.
- *Que se passe-t-il si un client ferme sa fenêtre ?* `Net.rcvMsg` lance une exception, attrapée par le `catch` de `doOperation()`, qui appelle `close()` : le client est retiré proprement.

---

## Chapitre 3 · Delegates et events — tag `COURS 3`

**À retenir**
- Un **delegate** est un type qui « pointe » vers une ou plusieurs méthodes ayant la même signature.
- Un delegate peut contenir **plusieurs** méthodes (multicast). Les appeler, c'est toutes les appeler, dans l'ordre.
- Un **event** = un delegate protégé : de l'extérieur, on peut seulement s'abonner (`+=`) ou se désabonner (`-=`). Seule la classe qui le déclare peut le déclencher.
- Le modèle du cours en 5 étapes :
  1. `public delegate void myDelegateClass(object sender, EventArgs e);`
  2. `public event myDelegateClass MyDel;` dans l'émetteur
  3. `protected virtual void onMyDel(object sender, EventArgs e) { if (MyDel != null) MyDel(sender, e); }`
  4. Une classe réceptrice avec une méthode de même signature (`action`)
  5. Abonnement : `t.MyDel += new myDelegateClass(r.action);`
- Le delegate transporte 2 infos : la **source** (`sender`) et la **nature** de l'événement (une classe qui hérite de `EventArgs`).

**Dans FinChat** (c'est la partie imposée par l'énoncé : « Broadcast messages … using delegates »)

| Étape du cours | FinChat |
|---|---|
| 1. delegate | `MessageDelegate(object sender, MessageEventArgs e)` dans `MessageEventArgs.cs` |
| `EventArgs` perso | `MessageEventArgs : EventArgs`, contient le message |
| 2. event | `public event MessageDelegate BroadcastEvent;` dans `Server.cs` |
| 3. méthode qui déclenche | `Server.onBroadcast(...)`, appelée par `broadcast()` |
| 4. méthode réceptrice | `Receiver.onBroadcast(object sender, MessageEventArgs e)` |
| 5. abonnement | `server.BroadcastEvent += new MessageDelegate(onBroadcast);` dans `Receiver.login()` |
| désabonnement | `server.BroadcastEvent -= onBroadcast;` dans `Receiver.close()` |

Aussi côté client : `buttonSend.Click += new System.EventHandler(this.buttonSend_Click);` dans `Form1.Designer.cs` est un abonnement à un event du bouton. Les delegates `displayDelegate` et `disconnectedDelegate` servent à `Invoke`.

**Questions possibles**
- *Pourquoi un event et pas une boucle sur la liste des clients ?* Le serveur n'a pas besoin de savoir qui écoute : il déclenche l'event, et chaque abonné réagit. C'est découplé : on peut ajouter un nouvel abonné (ex. un logger) sans toucher au code de diffusion.
- *Que se passe-t-il quand on appelle l'event ?* Toutes les méthodes abonnées sont appelées l'une après l'autre (multicast), ici `onBroadcast` de chaque `Receiver`.
- *Pourquoi le test `!= null` ?* Si personne n'est abonné, l'event vaut `null` : l'appeler planterait.
- *Pourquoi copier `BroadcastEvent` dans une variable avant le test ?* Un client peut se désabonner (`-=`) entre le test et l'appel, depuis un autre thread. La copie évite ce problème.
- *Où est filtré le channel ?* Dans `Receiver.onBroadcast()` : chaque abonné reçoit tout et garde seulement les messages de son channel.

---

## Chapitre 4 · Threads et networking — tags `COURS 4 - Threads`, `COURS 4 - Network`, `LAB 4 - lock`

**À retenir — threads**
- Un thread = un « processus léger ». Les threads d'une application partagent la même mémoire.
- Création : `Thread t = new Thread(new ThreadStart(methode)); t.Start();`
- Passer des données : une classe « avec état » (`ThreadWithState`) dont on lance une méthode.
- `Sleep()` met en pause, `Join()` attend la fin d'un thread, `Abort()` le détruit.
- Ressource partagée = **section critique** : un seul thread à la fois. Outils : `lock(objet)`, `Monitor.Enter/Exit`, `Semaphore` (`WaitOne` / `Release`), `Mutex`.
- L'objet de `lock` doit être **partagé** (donc `static`) entre les threads.

**À retenir — réseau**
- Serveur : `TcpListener` sur une IP et un port, `Start()`, puis `AcceptTcpClient()` (bloquant) renvoie un `TcpClient` par client.
- Client : `new TcpClient("serveur", port)`.
- On lit et écrit avec le flux `GetStream()`.
- **Serveur multi-clients** : un thread par client accepté (slide « Multi client server »).
- Un socket peut recevoir à tout moment : un thread lit en boucle `while (connexion valide)`.

**Dans FinChat**

| Notion | Où |
|---|---|
| `TcpListener` + `AcceptTcpClient()` | `Server.start()` et `Server.listen()` |
| 1 thread par client | `new Thread(new ThreadStart(r.doOperation)).Start();` dans `Server.listen()` |
| Classe avec état | `Receiver` : chaque objet connaît son `TcpClient`, son pseudo, son channel |
| `TcpClient` côté client | `Form1.buttonConnect_Click()` |
| Thread de réception côté client | `Form1.receive()` |
| `Thread.Sleep` + `Abort` | `FinanceNews.run()` et `stop()` |
| `lock(access_grant)` static | `Server.cs` (liste des clients), `Logger.log()` |
| `lock` par client | `Receiver.send()` |

**Questions possibles**
- *Combien de threads tournent sur le serveur ?* 1 principal (console) + 1 d'écoute + 1 par client + 1 finance.
- *Pourquoi un thread par client ?* `rcvMsg` est bloquant : sans thread, le serveur attendrait un seul client et ignorerait les autres.
- *Pourquoi un `lock` sur la liste des clients ?* Plusieurs threads la lisent et la modifient (connexion, déconnexion, `/users`). Sans verrou, on peut parcourir la liste pendant qu'un autre thread la modifie (comme le problème de « race » du Lab 4).
- *Pourquoi un `lock` dans `Receiver.send()` ?* Deux threads peuvent écrire en même temps dans le même socket (un message d'un utilisateur et un article finance). Les octets se mélangeraient et l'objet arriverait cassé.
- *Pourquoi `lock` et pas un sémaphore ?* `lock` suffit pour « un seul thread à la fois » et le verrou est relâché automatiquement à la fin du bloc. Un `Semaphore(1,1)` ferait la même chose.
- *Comment s'arrête le serveur ?* `listener.Stop()` débloque `AcceptTcpClient()` avec une exception, le thread d'écoute se termine, puis chaque client est fermé.

---

## Chapitre 5 · IO (entrées / sorties) — tag `COURS 5`

**À retenir**
- 2 sortes d'IO : **octets bruts** (`BinaryReader` / `BinaryWriter`) et **texte** (`StreamReader` / `StreamWriter`).
- `using (...) { }` ferme le flux automatiquement à la fin du bloc.
- `ReadLine()` renvoie `null` à la fin du fichier.

**Dans FinChat**
- `Logger.log()` : `new StreamWriter("server_log.txt", true)`, le `true` veut dire « ajouter à la fin ».
- `FinanceNews.readKey()` : `StreamReader` pour lire la clé API dans `finnhub_key.txt`.

**Questions possibles**
- *Pourquoi `using` ?* Le fichier est fermé même en cas d'erreur, donc il n'est pas bloqué pour le prochain log.
- *Pourquoi la clé est dans un fichier et pas dans le code ?* Pour ne pas la publier sur GitHub : le fichier est dans `.gitignore`.

---

## Chapitre 6 · Sérialisation — tag `COURS 6`

**À retenir**
- Sérialiser = transformer un objet en suite d'octets (pour un fichier ou le réseau). Désérialiser = l'inverse.
- Un objet n'est pas sérialisable par défaut : il faut **`[Serializable]`** sur la classe. `[NonSerialized]` exclut un attribut.
- Seuls les **attributs** sont sérialisés, pas les méthodes ni les propriétés.
- Formatters : `SoapFormatter` (XML), `BinaryFormatter` (plus compact).
- Héritage : la classe mère et la classe fille doivent toutes les deux être `[Serializable]`. Un tableau polymorphe est sérialisable.
- Sérialisation personnalisée : `ISerializable` + `GetObjectData()` + un constructeur spécial.

**Dans FinChat**
- Les 6 messages sont `[Serializable]` (`Message.cs`).
- `Net.sendMsg()` : `bf.Serialize(stream, msg)` écrit l'objet **directement dans le socket TCP**.
- `Net.rcvMsg()` : `bf.Deserialize(stream)` reconstruit l'objet de l'autre côté.
- `UserListMsg` contient un `string[]` : un tableau est sérialisable.

**Questions possibles**
- *Comment un objet passe d'un programme à l'autre ?* `BinaryFormatter` le transforme en octets, ils passent dans le flux TCP, et l'autre programme reconstruit l'objet.
- *Pourquoi un projet `Communication` séparé ?* Le client et le serveur doivent utiliser **exactement les mêmes** classes de messages, sinon la désérialisation échoue.
- *Pourquoi .NET Framework 4.8 et pas .NET 10 ?* `BinaryFormatter` a été supprimé de .NET 9/10 (jugé dangereux). Le squelette du prof l'utilise, donc on garde .NET Framework.
- *Limite à connaître :* `BinaryFormatter` n'est pas sûr avec des données venant d'inconnus (une donnée piégée peut exécuter du code). Ici on reste sur un réseau local, dans le cadre du cours.

---

## Chapitre 7 · Generics — tag (pas de tag dédié, voir `List<T>`)

**À retenir (slides)**
- Sans generics, on range des `object` (p3-6) : **boxing / unboxing** pour les types valeur, **casts** partout, et surtout **perte de la vérification de type** : les erreurs arrivent à l'exécution (« runtime bugs »).
- Une classe générique a un paramètre de type rempli à l'utilisation : `List<int>`, `LinkedList<K,T>` (plusieurs paramètres possibles).
- **Contraintes** : `where K : IComparable<K>` (le type doit avoir `CompareTo`), une classe de base (une seule, en premier), `new()` (constructeur par défaut).
- **`is` / `as`** plutôt qu'un cast direct (p18) : `is` teste le type, `as` caste ou renvoie `null`.
- Méthodes génériques, méthodes génériques `static`, events génériques.

**Dans FinChat**
- `List<Receiver> clients` dans `Server.cs`, `List<string> names` dans `getUsers()`.
- On n'écrit pas nos propres classes génériques : on n'en a pas besoin.
- `is` avant chaque cast dans `Receiver.doOperation()` et `Form1.display()`.

**Questions possibles**
- *Pourquoi `List<Receiver>` plutôt qu'`ArrayList` ?* Avec `ArrayList` il faudrait caster chaque élément `(Receiver)`, et une erreur de type ne se verrait qu'à l'exécution. `List<Receiver>` n'accepte que des `Receiver`.
- *`is` ou `as` ?* Les deux évitent le plantage d'un cast faux. On a gardé `is` + cast, la forme de la slide « checking class » (Ch2 p8-9).

---

## Chapitre 8 · Collections — tag `COURS 8`

**À retenir**
- Collections d'objets : `ArrayList`, `Queue`, `Stack`, `Hashtable`, `SortedList` (il faut caster en sortie).
- Collections génériques : `List<T>` (préférées) : `Add`, `Insert`, `Remove`, `RemoveAt`, `Contains`, `IndexOf`, `Count`, accès par `[i]`.

**Dans FinChat**
- `clients.Add(r)` à la connexion, `clients.Remove(r)` à la déconnexion, `clients.ToArray()` pour copier la liste avant de la parcourir sans verrou (`Server.stop()`).

---

## Lab 2 · Accès thread-safe à l'interface — tag `LAB 2 - Invoke`

**À retenir**
- Seul le thread qui a créé la fenêtre peut modifier ses contrôles. Sinon : `InvalidOperationException` (« Cross-thread operation not valid »).
- Solution : `if (InvokeRequired) Invoke(new monDelegate(maMethode), args);` : on demande au thread de l'interface d'exécuter la méthode.

**Dans FinChat**
- `Form1.display()` et `Form1.disconnected()` : le thread de réception ne touche jamais la `TextBox`, il passe par `Invoke`.

**Question possible**
- *Pourquoi `Invoke` dans le client ?* Les messages arrivent dans le thread de réception, mais la `TextBox` appartient au thread de la fenêtre. Sans `Invoke`, on aurait l'erreur du Lab 2.

---

## Hors cours — tag `HORS COURS`

Seulement dans `FinanceNews.getLastArticle()` :
- `WebClient.DownloadString(url)` : requête HTTP vers l'API Finnhub.
- `DataContractJsonSerializer` : transforme le JSON en tableau d'objets `Article`. C'est la même idée que la sérialisation du cours (`[DataContract]` / `[DataMember]` au lieu de `[Serializable]`).

**Question possible**
- *Pourquoi l'article passe par `broadcast()` ?* Pour réutiliser le même mécanisme d'event : FinanceBot est un expéditeur comme un autre, et les clients de #general l'ignorent grâce au filtre de channel.
