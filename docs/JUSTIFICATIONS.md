# Justifier nos choix à l'oral

Chaque choix du code, avec la raison prise **dans le cours**.

Chaque partie commence par un encadré **En clair (débutant)** : c'est quoi, où on s'en sert dans FinChat, et pourquoi on en a besoin ici. Ensuite, pour chaque choix :

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

## 0. Le projet en 1 minute (pour débutant)

**FinChat, c'est quoi ?** Un chat comme Discord, en très simple.
- Le **serveur** est un programme qui tourne en continu et attend. C'est comme un standard téléphonique.
- Chaque **client** est une fenêtre ouverte par un utilisateur. Il se connecte au serveur.
- Tous les messages passent par le serveur, qui les redistribue.
- Un robot, **FinanceBot**, poste un article de finance toutes les heures dans le channel **#finance**.

**Le chemin d'un message, étape par étape** (chaque étape = une notion du cours) :
1. Alice tape « salut » et clique sur Send → la fenêtre réagit au clic grâce à un **event** (partie 11).
2. Le client crée un objet `ChatMsg` (partie 3) et le transforme en octets pour l'envoyer : **sérialisation** (partie 4).
3. Les octets passent par la connexion **TCP** jusqu'au serveur (partie 1).
4. Côté serveur, un **thread** réservé à Alice attendait ses messages (partie 6). Il reconstruit l'objet et vérifie son type avec **`is`** (partie 3).
5. Le serveur déclenche son **event** de diffusion : tous les `Receiver` abonnés le reçoivent. Il y a un `Receiver` par client connecté, côté serveur (partie 5).
6. Chaque `Receiver` garde le message seulement si son client est dans le bon channel, puis le renvoie à son client par le réseau, protégé par un **`lock`** (partie 7).
7. Chez Bob, le thread de réception reçoit le message et demande à la fenêtre de l'afficher avec **`Invoke`** (partie 11).
8. Le serveur écrit une ligne dans **`server_log.txt`** (partie 9).
9. Si quelqu'un ferme sa fenêtre brutalement, un **`try` / `catch`** évite que le serveur plante (partie 8).

---

## 1. Organisation générale

> **En clair (débutant)**
> - **C'est quoi ?** Un programme **client-serveur** : le serveur attend, les clients se connectent à lui. Pour se trouver, il faut une **adresse IP** (quel ordinateur) et un **port** (quelle « porte » sur cet ordinateur). `127.0.0.1` veut dire « mon propre ordinateur ».
> - **Le squelette** = le code de départ donné par le prof (`DistantCalculator`). On l'a gardé et on a remplacé la calculatrice par un chat.
> - **Une DLL** = une boîte de code partagée. Ce n'est pas un programme qu'on lance : d'autres programmes l'utilisent. Notre DLL `Communication` contient les messages, utilisés à la fois par le serveur et par le client.
> - **Pourquoi ici ?** Pour que le client et le serveur parlent exactement la même langue, et pour rester fidèle au squelette du prof.

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

> **En clair (débutant)**
> - **Classe** = un moule. **Objet** = ce qu'on fabrique avec le moule. Exemple : `ChatMsg` est le moule, chaque message envoyé est un objet.
> - **Attribut** = une donnée rangée dans l'objet (le texte, l'expéditeur). **Méthode** = une action (envoyer, fermer).
> - **`private`** = un tiroir fermé à clé : seule la classe elle-même a la clé. **`public`** = ouvert à tout le monde.
> - **Propriété** (`get` / `set`) = un guichet devant le tiroir. `get` = « donne-moi la valeur », `set` = « remplace la valeur ». Sans `set`, on peut lire mais pas modifier.
> - **`static`** = qui appartient à la classe, pas à chaque objet : il n'en existe **qu'un seul exemplaire**, partagé par tous. **`const`** = une valeur qui ne change jamais.
> - **Où dans FinChat ?** `Message.cs` (attributs privés + `get`), `Logger.log()` (`static`), les constantes de `FinanceNews` et `Logger`.
> - **Pourquoi ici ?** Pour qu'aucune autre partie du code ne puisse modifier une donnée par erreur ou tricher, par exemple changer le nom de l'expéditeur d'un message.

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

> **En clair (débutant)**
> - **Interface** = une étiquette, un contrat. `Message` dit juste « je suis un message ». Les 6 classes de messages portent cette étiquette.
> - **Polymorphisme** = une variable de type `Message` peut contenir **n'importe lequel** des 6 messages. C'est comme un carton « colis » : dedans il peut y avoir un livre ou des chaussures.
> - **`is`** = « est-ce que c'est un `ChatMsg` ? » (réponse oui ou non). **Cast** `(ChatMsg)msg` = « d'accord, traite-le maintenant comme un `ChatMsg` ». On vérifie avec `is` **avant** de caster, sinon le programme peut planter.
> - **`override ToString()`** = chaque message explique lui-même comment il s'affiche en texte.
> - **Où dans FinChat ?** `Message.cs` (interface + 6 classes), `Receiver.doOperation()` et `Form1.display()` (tri avec `is`).
> - **Pourquoi ici ?** Quand un message arrive par le réseau, on ne sait pas encore de quel type il est. On l'ouvre comme un `Message`, puis on regarde ce qu'il y a dedans pour savoir quoi faire.

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

> **En clair (débutant)**
> - **Le problème :** un objet vit dans la mémoire d'un programme. Le réseau, lui, ne transporte que des **octets** (des 0 et des 1).
> - **Sérialiser** = démonter l'objet en octets pour l'envoyer. **Désérialiser** = le remonter à l'identique de l'autre côté. Comme un meuble démonté dans un carton, puis remonté chez quelqu'un d'autre.
> - **`[Serializable]`** = l'autorisation de démonter cet objet. Sans elle, .NET refuse.
> - **`BinaryFormatter`** = l'outil qui démonte et remonte.
> - **Où dans FinChat ?** `[Serializable]` sur les 6 messages (`Message.cs`) ; `Net.sendMsg()` (démonter + envoyer) et `Net.rcvMsg()` (recevoir + remonter).
> - **Pourquoi ici ?** C'est ce qui permet d'envoyer un vrai objet `ChatMsg` d'un programme à un autre, au lieu de bricoler du texte.

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

> **En clair (débutant)**
> - **Delegate** = une variable qui contient une **méthode** au lieu d'un nombre. C'est comme un numéro de téléphone noté dans un carnet : on pourra appeler plus tard.
> - Un delegate peut contenir **plusieurs** méthodes. L'appeler, c'est les appeler toutes, l'une après l'autre.
> - **Event** = une liste d'abonnés, comme une newsletter. `+=` = s'abonner, `-=` = se désabonner. Seul celui qui a créé l'event peut « envoyer la newsletter ».
> - **Où dans FinChat ?** `Server.BroadcastEvent` (la newsletter), `Receiver.onBroadcast()` (ce que fait chaque abonné quand il la reçoit), `+=` dans `login()`, `-=` dans `close()`.
> - **Pourquoi ici ?** Quand Alice écrit, le serveur déclenche l'event **une seule fois** et tous les clients connectés le reçoivent. Chacun garde le message seulement s'il est dans le bon channel. C'est exactement ce que demande le sujet : « broadcast using delegates ».

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

> **En clair (débutant)**
> - **Thread** = un « fil » d'exécution, comme un employé qui fait une tâche. Un programme simple a un seul employé, qui fait une chose à la fois. Avec plusieurs threads, plusieurs employés travaillent **en même temps**.
> - **Bloquant** = une instruction qui attend. `rcvMsg` attend qu'un message arrive : l'employé reste planté devant la porte et ne fait rien d'autre.
> - **`Sleep`** = faire dormir le thread un certain temps. **`Abort`** = arrêter le thread de force.
> - **Où dans FinChat ?** `Server.listen()` (attend les nouveaux clients), `Receiver.doOperation()` (un par client), `FinanceNews.run()` (le robot finance), `Form1.receive()` (réception côté client).
> - **Pourquoi ici ?** Le serveur doit écouter **plusieurs clients à la fois** et accepter de nouveaux clients en même temps. Avec un seul employé, il resterait bloqué à attendre le premier client. Côté client, sans thread de réception, la fenêtre serait gelée.

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

> **En clair (débutant)**
> - **Le problème :** plusieurs threads touchent la **même** chose en même temps, par exemple la liste des clients ou le même socket. C'est comme deux personnes qui écrivent sur la même feuille au même moment : ça donne un gribouillis.
> - **`lock`** = la clé des toilettes : un seul thread entre à la fois, les autres attendent leur tour. La clé est rendue **automatiquement** en sortant du bloc.
> - **Objet `static`** pour le `lock` = tout le monde utilise **la même** clé. Si chacun avait sa propre clé, le verrou ne servirait à rien.
> - **Où dans FinChat ?** `Server` (la liste des clients), `Receiver.send()` (un verrou par client), `Logger.log()` (le fichier de log).
> - **Pourquoi ici ?** Sans verrou, deux messages envoyés en même temps au même client arriveraient mélangés, et la liste des clients pourrait être abîmée.

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

> **En clair (débutant)**
> - **Exception** = une erreur qui arrive pendant que le programme tourne : connexion coupée, fichier absent, Internet en panne… Si on ne fait rien, **le programme s'arrête net**.
> - **`try`** = « essaie de faire ça ». **`catch`** = « si ça rate, fais plutôt ça ». C'est un filet de sécurité.
> - **Où dans FinChat ?** `Receiver.doOperation()` et `Form1.receive()` (déconnexion), `Server.listen()` (arrêt du serveur), `FinanceNews` (pas de clé, API en panne), `Logger` (fichier inaccessible).
> - **Pourquoi ici ?** Un client qui ferme sa fenêtre, c'est normal. Sans `try` / `catch`, ça ferait planter tout le serveur pour tout le monde.

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

> **En clair (débutant)**
> - **Stream (flux)** = un tuyau entre le programme et un fichier. **`StreamWriter`** écrit du texte dedans, **`StreamReader`** lit du texte.
> - **`using`** = ferme le tuyau automatiquement à la fin, même s'il y a une erreur.
> - **Où dans FinChat ?** `Logger.log()` écrit chaque événement dans `server_log.txt` ; `FinanceNews.readKey()` lit la clé API dans `finnhub_key.txt`.
> - **Pourquoi ici ?** Le sujet demande de garder une trace de l'activité du serveur (connexions, messages, erreurs). Et la clé API reste dans un fichier à part pour ne pas la publier sur GitHub.

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

> **En clair (débutant)**
> - **Collection** = une liste d'objets qui peut grandir ou rétrécir. Un tableau, lui, a une taille fixe.
> - **`List<Receiver>`** : ce qui est entre `< >` dit **quel type** on a le droit de mettre dedans. C'est une boîte étiquetée « seulement des `Receiver` ». C'est ça, un **generic**.
> - `Add` = ajouter, `Remove` = retirer, `Count` = combien il y en a.
> - **Où dans FinChat ?** `Server.clients` (les clients connectés), `getUsers()` (la liste des pseudos).
> - **Pourquoi ici ?** Le nombre de clients change tout le temps (connexions, déconnexions) : il faut une liste qui s'adapte. Et grâce au `< >`, impossible d'y mettre autre chose par erreur.

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

> **En clair (débutant)**
> - **Windows Forms** = la bibliothèque pour faire des fenêtres avec des boutons, des zones de texte, des listes.
> - **La règle :** seul le thread qui a créé la fenêtre a le droit de la modifier. C'est comme une voiture : seul le conducteur touche le volant.
> - **`Invoke`** = le thread de réception (le passager) donne le message au thread de la fenêtre (le conducteur) et lui demande de l'afficher.
> - **Les clics sont aussi des events :** cliquer sur Send déclenche l'event `Click`, et notre méthode `buttonSend_Click` est abonnée à cet event.
> - **Où dans FinChat ?** `Form1.cs` (`display()`, `disconnected()`, les méthodes `..._Click`), `Form1.Designer.cs` (les abonnements `+=`).
> - **Pourquoi ici ?** Les messages arrivent dans le thread de réception, mais c'est la fenêtre qui doit les afficher. Sans `Invoke`, on a l'erreur « cross-thread operation ».

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

> **En clair (débutant)**
> - **API** = un site web qui répond avec des **données** au lieu d'une page à afficher. Ici, Finnhub nous donne les dernières actualités financières.
> - **HTTP** = le langage du web (celui du navigateur). **JSON** = un format de texte pour les données, par exemple `{"headline": "...", "source": "Reuters"}`.
> - **`WebClient`** télécharge la réponse ; **`DataContractJsonSerializer`** transforme ce texte JSON en objet `Article`.
> - **Où dans FinChat ?** Uniquement dans `FinanceNews.getLastArticle()`.
> - **Pourquoi ici ?** Le sujet demande un article financier venant d'une API. Le cours ne montre pas HTTP ni JSON, donc on a gardé cette partie très petite et isolée.

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
