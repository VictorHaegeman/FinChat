# Plan de test

Configuration : 1 serveur + 2 ou 3 clients sur le même PC (`start-server.bat`, puis `start-client.bat` plusieurs fois).
Pour tester #finance rapidement, mettre `INTERVAL = 60000` (1 minute) dans `ChatServer/FinanceNews.cs`.

**Résultat** : `OK` = vérifié, `—` = encore à faire. Les lignes marquées *log 06/10* ont été vérifiées dans `server_log.txt` lors du premier test.

| # | Fonction (énoncé) | Étapes | Résultat attendu | Résultat | Capture |
|---|---|---|---|---|---|
| T1 | Démarrer le serveur | Lancer le serveur | `server started on port 8976` | OK *log 06/10* | `05-console-serveur.png` |
| T2 | Connexion + login | Client 1 : pseudo `alice`, Connect | `welcome alice` + alice dans la liste | OK *log 06/10* | |
| T3 | Plusieurs clients | Client 2 : `bob`, Connect | alice voit `bob joined the chat`, la liste contient alice et bob | OK *log 06/10* | `01-deux-clients.png` |
| T4 | Pseudo unique | Client 3 : pseudo `bob` | Refusé : `already taken`, client déconnecté | — | |
| T5 | Message public (diffusion par event) | alice tape `salut` | `[#general] alice : salut` chez alice et bob | OK *log 06/10* | `01-deux-clients.png` |
| T6 | Message privé | alice tape `/msg bob coucou` | Affiché chez alice et bob seulement | — | `02-message-prive.png` |
| T7 | Utilisateur inconnu | `/msg personne test` | `user personne not found` | — | |
| T8 | Changer de channel | alice choisit `finance` | `you joined #finance` + dernier article | OK *log 06/10* | `03-finance.png` |
| T9 | Filtre par channel | bob (#general) tape `hello` | alice (#finance) ne le reçoit pas | — | |
| T10 | Article finance automatique | Attendre l'intervalle | `[#finance] FinanceBot : titre (source) lien` | OK *log 06/10* | `03-finance.png` |
| T11 | `/users` | Taper `/users` | `connected users : alice, bob` | — | |
| T12 | `/name` | alice tape `/name alice2` | Tout le monde voit `alice is now alice2`, liste mise à jour | OK *log 06/10* | `04-commandes.png` |
| T13 | `/help` | Taper `/help` | Liste des commandes | — | `04-commandes.png` |
| T14 | `/quit` ou Disconnect | bob clique Disconnect | alice voit `bob left the chat`, liste mise à jour | OK *log 06/10* | |
| T15 | Fermeture brutale | Fermer la fenêtre de bob avec la croix | Le serveur logue la déconnexion, alice voit `bob left the chat` | — | |
| T16 | Arrêter le serveur | Taper `stop` dans la console | Tous les clients affichent `disconnected`, log `server stopped` | OK *log 06/10* | |
| T17 | Redémarrer | Taper `start`, reconnecter un client | Le chat refonctionne | — | |
| T18 | Logs | Ouvrir `server_log.txt` | Toutes les actions avec date et heure | OK *log 06/10* | `06-log.png` |
| T19 | Sans clé API | Renommer `finnhub_key.txt`, relancer | `no API key ... (#finance disabled)`, le chat marche | — | |
| T20 | Serveur absent | Client Connect sans serveur | `[error] cannot connect to the server` | — | |
