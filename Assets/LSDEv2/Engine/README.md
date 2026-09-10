# LSDE Dialog Engine 2.0.0 — assemblies

Ces deux DLL remplacent les paquets NuGet `LsdeDialogEngine 0.3.0` et
`LsdeDialogEngine.Newtonsoft 0.3.0`, retirés de `Assets/Packages/` et de `Assets/packages.config`.

| Fichier | Rôle |
|---|---|
| `LsdeDialogEngine.dll` | le moteur : `DialogueEngine`, `ISceneHandle`, `LsdeUtils`, les types du payload |
| `LsdeDialogEngine.Newtonsoft.dll` | `LsdeJson.Parse` — la lecture du JSON, via `Newtonsoft.Json` |
| `*.xml` | la documentation, pour l'IntelliSense |

## Pourquoi des DLL et pas les sources

Le moteur est écrit avec les types référence nullables activés (`<Nullable>enable</Nullable>`).
Unity compile `Assembly-CSharp` sans cette option, donc importer les `.cs` produit un `CS8632`
par annotation `?` — plusieurs centaines d'avertissements dans la console, sans aucun rapport
avec le jeu. Une DLL déjà compilée n'en produit aucun, exactement comme le paquet NuGet 0.3.0
qu'elle remplace.

## Régénérer après une modification du moteur

Depuis le dépôt du moteur (`LS-Dialog-Editor-Engine/lsde-csharp/Runtime/Newtonsoft`) :

```bash
dotnet build -c Release
```

puis copier `build~/bin/LsdeDialogEngine/Release/netstandard2.1/LsdeDialogEngine.{dll,xml}` et
`build~/bin/LsdeDialogEngine.Newtonsoft/Release/netstandard2.1/LsdeDialogEngine.Newtonsoft.{dll,xml}`
ici.

## Revenir à NuGet

`LsdeDialogEngine 2.0.0` n'est pas publié : nuget.org n'a que 0.1.0, 0.1.1, 0.2.0 et 0.3.0. Le
jour où la 2.0.0 sort, supprimer ce dossier et réinstaller les deux paquets avec NuGetForUnity.

## Version

Moteur **2.0.0**, format `lsde-blueprints` version 1 — ce qu'écrit LSDE 2.x. Un projet resté sur
LSDE 1.6 doit garder le moteur 0.3.x : les deux formats ne partagent aucun champ, il n'y a pas de
lecteur double ni de repli.
