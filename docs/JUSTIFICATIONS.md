# Justifier nos choix à l'oral

Chaque choix du code, avec la raison prise **dans le cours**. Pour chaque point :

- **Code** : ce qu'on a fait, et où.
- **Cours** : la slide qui le justifie.
- **Sinon** : l'autre solution possible et pourquoi on ne l'a pas prise.
- **À l'oral** : une phrase prête à dire.

Les références sont `Ch1 p11` = slide 11 du PDF « 1- Csharp basics ». Ch1 Basics, Ch2 Classification, Ch3 Delegates Events, Ch4 Threads Networking, Ch5 IO, Ch6 Serialization, Ch7 Generics, Ch8 Collections.

**Méthode pour répondre à n'importe quelle question :** « Le cours dit … (chapitre, slide). Donc on a fait … Sinon il se passerait … »

---

## Résumé en une page

| Choix | Cours | Phrase courte |
|---|---|---|
| Partir du squelette `DistantCalculator` | consigne du prof | Même structure : `Communication` + serveur + client. |
| `Communication` = bibliothèque (DLL) | Ch1 p47 | Le client et le serveur utilisent **les mêmes** classes, sinon la désérialisation échoue. |
| Attributs `private` | Ch1 p11, p27, p33 | Seule la classe peut modifier ses données. |
| Propriétés `get` sans `set` dans les messages | Ch1 p23-28 | Un message ne change plus après sa création. |
| `interface Message` | Ch2 p26, p33 | Les messages n'ont pas de code commun : un contrat suffit. |
| `is` puis cast | Ch2 p7-9 | Le cast seul peut planter : on vérifie d'abord le type. |
| `override ToString()` | Ch2 p1, p18 | Le client affiche n'importe quel message sans connaître son type. |
| `[Serializable]` + `BinaryFormatter` | Ch6 p2, p4, p11 | On envoie des objets entiers, en binaire compact. |
| `event` + delegate | Ch3 p14-22 | Le serveur prévient tous les abonnés sans les connaître. |
| `protected virtual onBroadcast` + test `!= null` | Ch3 p17 | Pas d'erreur s'il n'y a aucun abonné. |
| `+=` au login, `-=` à la fermeture | Ch3 p20-22 | Seuls les clients connectés reçoivent les messages. |
| 1 thread par client | Ch4 p33, p35 | `rcvMsg` bloque : un thread par client pour les écouter tous. |
| `Receiver` = classe avec état | Ch4 p10-13 | `ThreadStart` ne prend pas de paramètre : l'objet porte les données. |
| `lock` sur un objet `static` | Ch4 p18-20 | Un seul thread à la fois sur la liste ; le même verrou pour tous. |
| `lock` plutôt que `Semaphore` | Ch4 p25 | Même effet, et le verrou se libère tout seul. |
| `try` / `catch` | Ch2 p34-38 | Un client qui part ne doit pas faire tomber le serveur. |
| `StreamWriter` + `using` | Ch5 p2-4 | Log en texte lisible, fichier toujours refermé. |
| `List<Receiver>` | Ch8 p1-2, Ch7 p5-6 | Générique = pas de cast, erreurs vues à la compilation. |
| `Invoke` dans le client | Lab 2 | Seul le thread de la fenêtre peut modifier les contrôles. |
| HTTP + JSON | hors cours | Demandé par le sujet (API finance), isolé dans une seule méthode. |

---

## 1. Organisation générale

### 1.1 Partir du squelette `DistantCalculator`
- **Code :** 3 projets comme le squelette : `Communication` (messages + `Net`), `ChatServer` (`Server` + `Receiver`), `ChatClient` (fenêtre).
- **Cours :** le prof demande de partir d'un de ses squelettes. `DistantCalculator` est déjà un client-serveur TCP qui échange des objets.
- **Sinon :** repartir de zéro = plus de risque et moins de lien avec le cours.
- **À l'oral :** « On a gardé la structure du squelette et on a remplacé le calcul par le chat : `Expr` et `Result` sont devenus nos 6 messages. »

### 1.2 `Communication` est une bibliothèque séparée
- **Code :** `Communication.csproj` produit une DLL, référencée par le serveur et par le client.
- **Cours :** Ch1 p47, `csc /t:library` crée une `.dll` réutilisable par plusieurs programmes.
- **Sinon :** copier `Message.cs` dans les deux projets. Les classes auraient le même nom mais seraient dans deux assemblies différentes, et `BinaryFormatter` refuserait de reconstruire l'objet de l'autre côté.
- **À l'oral :** « Le client et le serveur doivent parler exactement la même langue : les classes de messages sont donc dans une seule DLL partagée. »

### 1.3 .NET Framework 4.8
- **Code :** `<TargetFramework>net48</TargetFramework>` dans les 3 projets.
- **Cours :** Ch6 p11 (`BinaryFormatter`) et Ch4 p14 (`Abort`). Les deux n'existent plus ou ne marchent plus dans .NET 9/10.
- **Sinon :** .NET 10 nous obligerait à remplacer `BinaryFormatter` et `Abort` par des choses qui ne sont pas dans le cours.
- **À l'oral :** « On a choisi .NET Framework pour pouvoir utiliser exactement les outils du cours et du squelette. »

### 1.4 Adresse `127.0.0.1`
- **Code :** `new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port)` dans `Server.start()`.
- **Cours :** Ch4 p31, « Sample server code » : `byte[] local_IP = {127,0,0,1};`.
- **Sinon :** `IPAddress.Any` pour accepter d'autres PC. C'est une seule ligne à changer.
- **À l'oral :** « On écoute en local comme dans l'exemple du cours ; pour un vrai réseau on mettrait `IPAddress.Any`. »

---

## 2. Classes et encapsulation (Ch1)

### 2.1 Tous les attributs sont `private`
- **Code :** partout : `private string text;`, `private List<Receiver> clients;`, `private TcpClient comm;`…
- **Cours :**
  - Ch1 p11 : « private is the default for members ».
  - Ch1 p27 : les propriétés servent à un accès **sûr** (« safe access / mutation »).
  - Ch1 p31-33 : depuis une autre classe, un attribut `private` n'est « même pas proposé » ; seule la classe elle-même peut y toucher.
  - L'exemple serveur du prof fait pareil : `private TcpListener ear;` (Ch4 p31).
- **Sinon :** des attributs `public`. N'importe quelle classe pourrait alors changer `clients` sans `lock`, ou changer l'expéditeur d'un message.
- **À l'oral :** « Une donnée n'est modifiée que par sa propre classe. Par exemple la liste des clients n'est changée que dans `Server`, toujours sous `lock`. »

### 2.2 Messages : propriétés en lecture seule (`get` sans `set`)
- **Code :** `public string Sender { get { return sender; } }`. Les valeurs sont données une fois, dans le constructeur.
- **Cours :** Ch1 p23-28 : une propriété peut avoir `get` et/ou `set`. Sans `set`, on peut lire mais pas écrire.
- **Pourquoi c'est important chez nous :** le serveur ne fait pas confiance au client. Il ne modifie pas le message reçu, il en **crée un nouveau** avec le vrai pseudo : `new ChatMsg(username, channel, msg.Text)` dans `Receiver.publicMessage()`. Un client ne peut donc pas se faire passer pour un autre.
- **Sinon :** avec un `set`, n'importe quel code pourrait changer un message après sa création, même pendant qu'un autre thread l'envoie.
- **À l'oral :** « Un message est créé, envoyé, lu, mais jamais modifié. C'est pour ça qu'il n'a que des `get`. »

### 2.3 `private` n'empêche pas l'envoi sur le réseau
- **Cours :** Ch6 p4 : « properties and methods : not serialized ; attributes : may be serialized ». `BinaryFormatter` copie les attributs, même `private`.
- **À l'oral :** « Le `private` protège dans le code, pas à la sérialisation : les attributs privés partent bien dans le message. »

### 2.4 `Receiver.Username` a un `set` public : pourquoi ?
- **Code :** `Server.reserveName()` vérifie que le pseudo est libre **et** le donne au client, dans le **même** `lock`.
- **Cours :** Ch4 p18 : section critique = « exclusive execution ». Vérifier puis modifier doit se faire d'un seul bloc.
- **Sinon :** si `Receiver` vérifiait d'abord puis changeait son nom après, deux clients pourraient vérifier « bob est libre » en même temps et prendre tous les deux « bob » (le problème de course du Lab 4).
- **À l'oral :** « Le `set` est appelé seulement par `Server`, à l'intérieur du verrou, pour que deux clients ne prennent jamais le même pseudo. »

### 2.5 `Logger.log()` est `static`
- **Code :** `public static void log(string text)`, appelé `Logger.log("...")` depuis toutes les classes.
- **Cours :** Ch1 p9-10 : une méthode `static` s'appelle avec le nom de la classe. « static methods can only access static members » : donc son verrou `access_grant` est aussi `static`.
- **Sinon :** créer un objet `Logger` et le passer à `Server`, `Receiver`, `FinanceNews`… pour rien, car il n'a pas d'état propre.
- **À l'oral :** « Le log n'a pas besoin d'objet, c'est une méthode de classe, comme la classe `P` du cours. »

### 2.6 Constantes `const`
- **Code :** `const string fileName = "server_log.txt"`, `KEY_FILE`, `URL`, `INTERVAL`.
- **Cours :** Ch5 p6 : `const string fileName = "AppSettings.dat";`.
- **À l'oral :** « Les valeurs fixes sont des constantes en haut de la classe : on les change à un seul endroit. »

---

## 3. Les messages : interface et polymorphisme (Ch2)

### 3.1 `interface Message` plutôt qu'une classe abstraite
- **Code :** `public interface Message { string ToString(); }`, implémentée par les 6 messages.
- **Cours :**
  - Ch2 p26 : une interface est un **contrat** : seulement des signatures, pas d'attributs.
  - Ch2 p33 : « use interfaces to specify behaviors ; use abstract classes when you have to write generic code ».
- **Pourquoi :** nos messages n'ont **aucun code commun** (ils n'ont pas les mêmes attributs : `LoginMsg` a juste un pseudo, `UserListMsg` un tableau). Une interface suffit. C'est aussi le choix du squelette.
- **Bonus sérialisation :** Ch6 p22 : avec une classe mère, elle devrait aussi être `[Serializable]`. Une interface n'a pas d'attributs, donc rien à sérialiser.
- **À l'oral :** « `Message` dit seulement "ceci est un message". Comme il n'y a pas de code à partager, le cours dit d'utiliser une interface. »

### 3.2 Pourquoi `ToString()` est dans l'interface alors que `object` l'a déjà ?
- **Cours :** Ch2 p1 : toute classe hérite de `object`, qui a déjà `ToString()`.
- **Réponse honnête :** ça ne change rien techniquement. On le met pour **écrire le contrat** : tout message doit savoir s'afficher. Le squelette faisait pareil.

### 3.3 `is` puis cast pour trier les messages reçus
- **Code :** `if (msg is ChatMsg) publicMessage((ChatMsg)msg);` dans `Receiver.doOperation()` et `Form1.display()`.
- **Cours :**
  - Ch2 p7 : le cast explicite `(horse)m` est accepté par le compilateur mais « can produce execution errors ».
  - Ch2 p8-9 (« checking class ») : on teste avec `is` avant de caster. Notre code est exactement l'exemple `if (a is snake) ((snake)a).doSomething();`.
- **Sinon 1 :** `as` (Ch7 p18), qui renvoie `null` si le type ne va pas. Même résultat, on a gardé la forme de la slide « checking class ».
- **Sinon 2 :** une méthode `virtual` `traiter()` dans chaque message (late binding, Ch2 p33 « use late binding as much as possible »). On ne l'a pas fait parce que les messages sont dans la DLL `Communication`, partagée avec le client : ils devraient connaître `Server` et `Receiver`, qui sont du code serveur.
- **À l'oral :** « `rcvMsg` renvoie un `Message` sans dire lequel. On vérifie le vrai type avec `is`, puis on caste, comme dans la slide "checking class". »

### 3.4 `override ToString()` dans chaque message = late binding
- **Code :** le client affiche `addLine(msg.ToString())` sans connaître le type.
- **Cours :** Ch2 p14-15 : avec `virtual` / `override`, la méthode appelée dépend du type **réel** de l'objet. Ch2 p18 : `public override string ToString() // override allows polymorphism`.
- **Sinon :** sans `override`, on masquerait la méthode (`new`, Ch2 p13) et ce serait le `ToString()` de `object` qui serait appelé (il afficherait juste le nom de la classe).
- **À l'oral :** « Le client appelle juste `ToString()` : grâce à `override`, c'est la bonne version qui s'exécute pour chaque type de message. »

### 3.5 `MessageEventArgs : EventArgs` et `: base()`
- **Cours :** Ch3 p18 : « you may also create your own EventArgs with inheritance ». Ch2 p16 : `base` appelle le constructeur de la classe mère.
- **À l'oral :** « Le `EventArgs` de base ne transporte rien ; on en hérite pour y mettre le message à diffuser. »

---

## 4. Sérialisation (Ch6)

### 4.1 `[Serializable]` sur chaque message
- **Cours :** Ch6 p2 : « objects are not serializable by default ». Ch6 p4-5 : on ajoute `[Serializable]` à la classe.
- **Sinon :** exception à l'envoi (`SerializationException`).
- **À l'oral :** « Sans `[Serializable]`, .NET refuse de transformer l'objet en octets. »

### 4.2 `BinaryFormatter` plutôt que `SoapFormatter`
- **Cours :** Ch6 p6 (Soap, XML) et Ch6 p11 : « Use the BinaryFormatter class for more efficient and compact serialization. Less verbose than XML and SOAP ».
- **À l'oral :** « Pour le réseau on veut des messages petits : le cours conseille le binaire. »

### 4.3 Envoyer des objets plutôt que des lignes de texte
- **Cours :** Ch4 p34 montre aussi `StreamReader` / `StreamWriter` sur le socket (texte, `ReadLine`). Ch4 p36 : une application réseau a besoin d'un « communication protocol dedicated to the application ».
- **Notre protocole = les 6 classes de messages.** Le type de l'objet dit déjà ce que c'est.
- **Sinon :** avec du texte, il faudrait inventer un format (`MSG|bob|general|salut`) et le découper à la main, avec des bugs si le texte contient le séparateur.
- **À l'oral :** « Notre protocole, ce sont nos classes : on envoie un `ChatMsg`, il arrive comme un `ChatMsg`. »

### 4.4 Pas de `ISerializable` ni de `[NonSerialized]`
- **Cours :** Ch6 p12-16 : `ISerializable` sert à une sérialisation **personnalisée**. `[NonSerialized]` (Ch6 p4) exclut un attribut.
- **Pourquoi pas :** tous nos attributs doivent partir, tels quels. La sérialisation automatique suffit.

### 4.5 `string[]` dans `UserListMsg`
- **Cours :** Ch6 p22 : « arrays are serializable ».
- **À l'oral :** « On envoie une photo de la liste à un instant donné : un simple tableau. »

---

## 5. Delegates et events (Ch3) — le cœur du sujet

Le sujet demande : *« Broadcast messages to all connected clients using delegates »*.

### 5.1 Le delegate `MessageDelegate(object sender, MessageEventArgs e)`
- **Cours :** Ch3 p13-14 : l'event a besoin de 2 informations, sa **source** (`sender`) et sa **nature** (`EventArgs`). Ch3 p14 : `public delegate void myDelegateClass(object sender, EventArgs e);`.
- **À l'oral :** « On a repris la signature du cours : qui envoie, et quoi. »

### 5.2 Le mot-clé `event` plutôt qu'un delegate public
- **Code :** `public event MessageDelegate BroadcastEvent;` dans `Server`.
- **Cours :** Ch3 p16 : `event` est un « scope modifier » : de l'extérieur, on peut seulement `+=` et `-=`. Seule la classe qui le déclare peut le déclencher.
- **Sinon :** avec un delegate public, un `Receiver` pourrait déclencher lui-même la diffusion, ou écrire `BroadcastEvent = ...` et **effacer tous les autres abonnés**.
- **À l'oral :** « Avec `event`, les clients peuvent s'abonner ou se désabonner, mais seul le serveur peut diffuser. »

### 5.3 `protected virtual void onBroadcast(...)` avec le test `!= null`
- **Cours :** Ch3 p17 : « for inheritance purposes, first create a protected virtual method raising the event » et `if (myDel != null) // check for subscribers`.
- **Pourquoi `!= null` :** s'il n'y a aucun abonné (personne de connecté), l'event vaut `null`, et l'appeler lancerait une `NullReferenceException`.
- **Petit ajout au cours :** on copie d'abord l'event dans une variable locale (`MessageDelegate subscribers = BroadcastEvent;`). Entre le test et l'appel, un autre thread peut se désabonner (dernier client qui part) : la copie, elle, ne devient pas `null`.
- **À l'oral :** « C'est la méthode `onMyDel` du cours. La copie locale est là parce que chez nous plusieurs threads s'abonnent et se désabonnent. »

### 5.4 `broadcast()` public
- **Cours :** Ch3 p18 : une méthode publique `raiseEvent()` qui appelle `onMyDel(this, ...)`.
- **Chez nous :** `Receiver` (messages des utilisateurs) **et** `FinanceNews` (articles) appellent le même `broadcast()`. Un seul circuit pour tout.

### 5.5 `+=` au login, `-=` à la fermeture
- **Code :** `server.BroadcastEvent += new MessageDelegate(onBroadcast);` dans `Receiver.login()`, `server.BroadcastEvent -= onBroadcast;` dans `Receiver.close()`.
- **Cours :** Ch3 p20 (abonnement) et p22 (« unsubscribing : `t.MyDel -= r.action;` »).
- **Pourquoi après le login :** un client qui n'a pas encore de pseudo ne doit rien recevoir.
- **Pourquoi `-=` :** Ch3 p8 : le delegate garde une référence vers l'objet. Sans `-=`, le serveur continuerait d'appeler un client fermé, et l'objet ne serait jamais libéré.
- **À l'oral :** « On s'abonne quand on entre dans le chat, on se désabonne quand on sort. »

### 5.6 Pourquoi un event plutôt qu'une boucle `foreach` sur les clients ?
- **Cours :** Ch3 p11-12 : la programmation par events permet de réagir sans attendre ni boucler. Ch3 p9 : un delegate multicast appelle **toutes** ses méthodes, dans l'ordre.
- **Chez nous :** le serveur n'a pas besoin de savoir qui écoute. **Chaque `Receiver` décide** s'il garde le message : `Receiver.onBroadcast()` jette un `ChatMsg` qui n'est pas de son channel.
- **À l'oral :** « Le serveur lance l'event une fois, tous les abonnés le reçoivent, et chacun filtre selon son channel. »

### 5.7 Les messages privés n'utilisent pas l'event
- **Code :** `Server.sendPrivate()` trouve le destinataire et appelle directement `target.send(msg)`.
- **Pourquoi :** on connaît le destinataire ; prévenir tout le monde pour qu'un seul garde le message serait inutile.

---

## 6. Threads (Ch4)

### 6.1 Un thread d'écoute
- **Code :** `new Thread(new ThreadStart(listen)).Start();` dans `Server.start()`.
- **Cours :** Ch4 p31 : `AcceptTcpClient(); // blocking`. Ch4 p5-7 : création d'un thread avec `ThreadStart`.
- **Sinon :** le thread principal resterait bloqué dans `AcceptTcpClient()` et ne lirait plus les commandes `start` / `stop` / `exit`.

### 6.2 Un thread par client
- **Code :** `new Thread(new ThreadStart(r.doOperation)).Start();` dans `Server.listen()`.
- **Cours :** Ch4 p33 (« Multi client server » : TcpListener → TcpClient → **Thread**), Ch4 p35 (un socket peut recevoir à tout moment : un thread qui lit en boucle `while (connection is valid)`).
- **Sinon :** `rcvMsg` est bloquant. Avec un seul thread, le serveur attendrait le premier client et ignorerait les autres.
- **À l'oral :** « Chaque client a son thread qui l'écoute, comme sur la slide "Multi client server". »

### 6.3 `Receiver` = classe avec état
- **Cours :** Ch4 p10-13 (« Parameterized Thread », classe `ThreadWithState`) : on met les données dans un objet, et le thread lance une méthode de cet objet.
- **Chez nous :** chaque `Receiver` garde **son** `TcpClient`, **son** pseudo, **son** channel. Le thread lance `r.doOperation`.
- **Sinon :** `ParameterizedThreadStart` (Ch4 p8) avec un paramètre `object` qu'il faut caster. Moins clair.
- **À l'oral :** « `ThreadStart` ne prend pas de paramètre, donc c'est l'objet `Receiver` qui porte les données du client. »

### 6.4 Thread `FinanceNews` avec `Thread.Sleep`
- **Code :** boucle `sendLastArticle(); Thread.Sleep(INTERVAL);`.
- **Cours :** Ch4 p17 : `sleep()` fait partie des outils de synchronisation.
- **Sinon :** un `Timer`, qui n'est pas dans le cours.

### 6.5 Arrêt avec `Abort()`
- **Code :** `thread.Abort();` dans `FinanceNews.stop()`, et `catch (ThreadAbortException) { throw; }` dans `sendLastArticle()`.
- **Cours :** Ch4 p14 (« Destroying a Thread ») : `Abort` lance une `ThreadAbortException` dans le thread, qui **peut l'attraper**. Ch2 p38 : on peut relancer une exception avec `throw` dans un `catch`.
- **Pourquoi le `catch` spécial :** sinon notre `catch (Exception)` l'attraperait et écrirait un faux « finance API error » dans le log.
- **Sinon :** un booléen `running` testé dans la boucle. Mais le thread dort 1 heure : il ne verrait le changement qu'au réveil.

### 6.6 Un thread de réception dans le client
- **Code :** `receiveThread = new Thread(new ThreadStart(receive));` dans `Form1`.
- **Cours :** Ch4 p35.
- **Sinon :** si la fenêtre attendait elle-même les messages, elle serait gelée (plus de clic, plus de saisie).

### 6.7 Combien de threads ?
- Serveur : principal (console) + écoute + 1 par client + finance.
- Client : fenêtre + réception.

---

## 7. Synchronisation (Ch4, Lab 4)

### 7.1 `lock` sur la liste des clients
- **Code :** `lock (access_grant)` dans `addClient`, `removeClient`, `reserveName`, `getUsers`, `findClient`.
- **Cours :** Ch4 p18 : plusieurs threads partagent une ressource → **section critique**, un seul à la fois, avec `lock`. Ch4 p22 : un thread qui arrive pendant le verrou attend son tour.
- **Sinon :** un client se connecte (`Add`) pendant qu'un autre thread parcourt la liste (`foreach`) → `InvalidOperationException` (« Collection was modified ») ou résultat faux.
- **À l'oral :** « La liste est utilisée par tous les threads ; on la protège avec `lock`, comme au Lab 4. »

### 7.2 L'objet du `lock` est `static`
- **Cours :** Ch4 p20 : « problem : access_grant is local to each thread : use a static object to share among threads ».
- **Réponse honnête si on insiste :** il n'y a qu'un seul objet `Server`, donc un attribut normal marcherait aussi. On suit la slide : `static` garantit qu'il n'existe **qu'un seul** verrou.

### 7.3 Un `lock` par client pour l'envoi (`send_grant`, pas `static`)
- **Code :** `lock (send_grant)` dans `Receiver.send()`.
- **Pourquoi :** deux threads peuvent écrire en même temps dans le **même** socket (le message de Bob et l'article de FinanceBot). Les octets se mélangeraient et l'objet arriverait cassé.
- **Pourquoi pas `static` :** la règle de Ch4 p20 est que tous les threads utilisent **le même objet verrou**. C'est le cas : tous passent par le même `Receiver`. Un verrou `static` bloquerait l'envoi vers **tous** les clients à chaque envoi, pour rien.
- **À l'oral :** « Un verrou par socket : on n'attend que si on écrit au même client. »

### 7.4 `lock` plutôt que `Semaphore`, `Mutex` ou `Monitor`
- **Cours :** Ch4 p23-24 (`Monitor.Enter/Exit`), Ch4 p25-26 (`Semaphore`, `WaitOne` / `Release` ; un `Mutex` = sémaphore à 1 place).
- **Pourquoi `lock` :** on veut « un seul thread à la fois », c'est exactement `lock`. Il libère le verrou tout seul à la fin du bloc, **même en cas d'exception**. Avec un sémaphore, un `Release()` oublié ou sauté par une exception bloque tout le monde.
- **À l'oral :** « Un sémaphore sert à laisser entrer N threads. On n'en veut qu'un : `lock` suffit et ne peut pas rester fermé par erreur. »

### 7.5 `stop()` copie la liste avant de fermer les clients
- **Code :** `copy = clients.ToArray();` sous `lock`, puis `foreach (Receiver r in copy) r.close(...)`.
- **Pourquoi :** `close()` retire le client de la liste (`removeClient`). Retirer un élément pendant un `foreach` sur la même liste lance une exception.

### 7.6 `lock` dans `Logger`
- **Pourquoi :** deux threads qui écrivent dans `server_log.txt` en même temps → lignes mélangées ou fichier déjà ouvert.

### 7.7 Le booléen `closed`
- **Pourquoi :** `close()` peut être appelé deux fois (fin du thread du client + `stop` du serveur). Il est testé et changé sous `lock` : le client n'est fermé qu'une fois.

---

## 8. Exceptions (Ch2)

### 8.1 `try` / `catch` autour de la réception
- **Code :** `Receiver.doOperation()`, `Form1.receive()`.
- **Cours :** Ch2 p34 (« defensive coding ») : prévoir les événements extérieurs, comme « connection lost ». Ch2 p35 : une exception non attrapée « brutally end program execution ».
- **Point fort :** sans ce `catch`, un client qui ferme sa fenêtre ferait planter **tout le serveur** pour tout le monde.
- **À l'oral :** « Une déconnexion n'est pas un bug, c'est un événement normal : on l'attrape et on ferme proprement ce client. »

### 8.2 Le plus précis avant le plus général
- **Code :** `catch (ThreadAbortException)` puis `catch (Exception)` dans `FinanceNews`.
- **Cours :** Ch2 p38 : « More specialized exceptions should be specified before the more general ones ».

### 8.3 Fin normale de `listen()`
- **Code :** `listener.Stop()` débloque `AcceptTcpClient()` avec une exception, attrapée dans `listen()`.
- **À l'oral :** « C'est la façon normale d'arrêter un `AcceptTcpClient()` bloqué. »

### 8.4 Pas de clé API ou API en panne
- **Code :** `FinanceNews.run()` et `sendLastArticle()` attrapent l'erreur et l'écrivent dans le log.
- **Pourquoi :** le chat doit marcher même sans Internet ou sans clé. Seul #finance est désactivé.

---

## 9. Fichiers (Ch5)

### 9.1 `StreamWriter` pour le log
- **Code :** `new StreamWriter("server_log.txt", true)` (`true` = ajouter à la fin).
- **Cours :** Ch5 p2 : `StreamReader` / `StreamWriter` = IO « interpreted » (texte). Ch5 p5 : `BinaryReader` / `BinaryWriter` = IO « raw » (octets).
- **Pourquoi texte :** un log doit se lire avec un éditeur de texte.

### 9.2 `using`
- **Cours :** Ch5 p3 : « The using statement also closes the StreamReader ».
- **Pourquoi :** le fichier est refermé même en cas d'erreur, donc il n'est pas bloqué pour la ligne suivante.

### 9.3 La clé API dans un fichier
- **Code :** `StreamReader` lit `finnhub_key.txt`, qui est dans `.gitignore`.
- **Pourquoi :** une clé écrite dans le code partirait sur GitHub.

---

## 10. Collections et generics (Ch7, Ch8)

### 10.1 `List<Receiver>` plutôt qu'`ArrayList`
- **Cours :**
  - Ch8 p1 : les collections d'objets (`ArrayList`…) obligent à caster en sortie (« Typecast on removal »).
  - Ch8 p2 : « Generics offer typesafe alternatives… generally the preferred alternative ».
  - Ch7 p5-6 : sans generics, casts partout, perte de la vérification à la compilation, « runtime bugs ».
- **À l'oral :** « `List<Receiver>` n'accepte que des `Receiver` : pas de cast, et une erreur de type se voit à la compilation. »

### 10.2 `List` plutôt que `Dictionary<string, Receiver>`
- **Cours :** Ch8 p27-32 : un dictionnaire range des paires clé → valeur, avec des clés uniques.
- **Pourquoi pas :** la clé serait le pseudo. Mais un client n'a pas encore de pseudo avant le login, et peut en changer avec `/name` : il faudrait l'enlever et le remettre à chaque fois. Avec quelques clients, parcourir une `List` est simple et suffit.

---

## 11. Le client Windows Forms (Lab 2)

### 11.1 `InvokeRequired` + `Invoke`
- **Code :** `display()` et `disconnected()` dans `Form1`.
- **Cours :** Lab 2 exo 2 : seul le thread qui a créé la fenêtre peut modifier ses contrôles, sinon `InvalidOperationException` (« cross-thread operation »).
- **À l'oral :** « Le thread de réception ne touche pas la fenêtre : il demande au thread de la fenêtre de le faire avec `Invoke`. »

### 11.2 Les delegates `displayDelegate` et `disconnectedDelegate`
- **Cours :** Ch3 p3-4 : on crée une classe delegate puis un objet `new displayDelegate(display)`. `Invoke` a besoin d'un delegate pour savoir quelle méthode lancer.

### 11.3 Les boutons utilisent aussi des events
- **Code :** `buttonSend.Click += new EventHandler(buttonSend_Click);` dans `Form1.Designer.cs`.
- **Cours :** Ch3 p20 : c'est exactement l'abonnement du cours. Le bouton est l'émetteur, la fenêtre est l'abonnée.

### 11.4 Le client envoie `ChatMsg(null, channel, text)`
- **Pourquoi `null` :** c'est le serveur qui écrit le vrai expéditeur (voir 2.2). Le client ne peut pas mentir sur son nom.

### 11.5 `using Message = Communication.Message;`
- **Pourquoi :** Windows Forms a aussi une classe `Message`. On dit au compilateur laquelle on veut (sinon erreur CS0104).

---

## 12. Hors cours (à assumer)

### 12.1 Requête HTTP et lecture du JSON
- **Code :** uniquement dans `FinanceNews.getLastArticle()` : `WebClient.DownloadString` puis `DataContractJsonSerializer`.
- **Pourquoi :** le sujet demande un article financier venant d'une API. Le cours ne montre pas HTTP (Ch4 p36 le cite seulement comme « existing protocol »).
- **Comment on reste proche du cours :** `[DataContract]` / `[DataMember]` = même idée que `[Serializable]` (Ch6) : transformer du texte JSON en objet `Article`. Les deux classes sont fournies avec .NET Framework, aucun paquet à installer.
- **À l'oral :** « C'est la seule partie hors cours. On l'a isolée dans une méthode, et l'article repart ensuite par le même `broadcast()` que les messages normaux. »

### 12.2 Le pseudo « financebot » est réservé
- **Pourquoi :** personne ne peut se faire passer pour le bot.

---

## 13. Questions pièges (réponses honnêtes)

- **« `BinaryFormatter` est dangereux, non ? »** Oui, Microsoft le déconseille : un message piégé peut exécuter du code. Ici on reste en local, dans le cadre du cours et du squelette. Pour une vraie appli on enverrait du JSON.
- **« `Thread.Abort` est obsolète. »** Oui, dans .NET récent. Dans .NET Framework 4.8 il marche, et c'est l'outil de la slide « Destroying a Thread ». Avec un `Sleep` d'une heure, c'est le moyen le plus simple d'arrêter le thread tout de suite.
- **« Pourquoi pas `async` / `await` ou `Task` ? »** Ce n'est pas dans le cours. Le cours montre `Thread` + `ThreadStart`.
- **« Dans quel thread s'exécute `Receiver.onBroadcast` de Bob quand Alice écrit ? »** Dans le thread d'**Alice**. Un event, c'est un appel de méthode normal : le thread qui déclenche l'event exécute toutes les méthodes abonnées. C'est pour ça qu'il faut le `lock` dans `send()`.
- **« Que se passe-t-il si deux clients écrivent au même moment ? »** Les deux threads déclenchent l'event en parallèle. Pour chaque destinataire, `send()` est verrouillé : les deux messages arrivent entiers, l'un après l'autre.
- **« `Logger.access_grant` et `Server.access_grant`, c'est le même verrou ? »** Non : deux attributs `static` dans deux classes différentes. Chacun protège sa propre ressource (le fichier, la liste).
- **« Pourquoi les noms de méthodes en minuscules (`start`, `doOperation`) ? »** On a gardé le style du squelette du prof.
- **« Il n'y a pas de mot de passe ? »** Non, le sujet ne le demande pas. On vérifie seulement que le pseudo est valide et libre.
