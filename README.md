# step

## export data from LSDE to Unity

exporter les scenes bleuprints de LSDE vers un dossier exclusivement pour les blueprints
ex: Assets/LSDE/blueprints
Ces fichiers serviront à être interprétés par LSDEDE runtime

## nuget for unity

installer nutget pour unity pour installer LSDEDE runtime
Click + button on the upper-left of a window, and select "Add package from git URL..."
https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
Chercher lsdeDialogEngine et lsdeDialogEngine.newtonSoft pour le deserialisation et le mapper polymorphique

##

1. Placer les 5 personnages
   Dans le panneau Project (en bas de l'écran Unity), navigue vers :
   Assets/characters/Tasty_Characters - Forest Pack/Ressources/Prefabs/

Glisse ces 5 prefabs dans le panneau Hierarchy (la liste à gauche) :

Chara_00 → sera l1
Chara_01 → sera l2
Chara_02 → sera l3
Chara_03 → sera l4 (joueur)
Chara_04 → sera boss
Positionne-les en ligne ou demi-cercle (ex: X=-3, -1.5, 0, 1.5, 3).

2. Sur chaque personnage :
   Add Component → tape DialogueCharacterMarker
   Remplis le champ Lsde Character Id : l1, l2, l3, l4, boss
   Clic droit sur le personnage dans Hierarchy → Create Empty → renomme BubbleAnchor
   Monte-le au-dessus de la tête (Y ≈ 2)
   Glisse BubbleAnchor dans le champ Bubble Anchor Point du marker


##interactable
Setup dans Unity Editor
1. Créer le layer "Interactable"
Menu Edit → Project Settings → Tags and Layers
Dans la section Layers, trouve un slot vide (ex: Layer 6) et tape Interactable
2. Créer le tag "Player"
Même fenêtre, section Tags → clique le + → tape Player
3. Configurer le joueur (l4)
Dans le Hierarchy (liste des objets à gauche), clique sur ton personnage joueur
Dans l'Inspector (panneau à droite) :
En haut, dans le dropdown Tag → sélectionne Player
Add Component → cherche Rigidbody → ajoute-le
Coche Is Kinematic = true
Décoche Use Gravity = false
Add Component → cherche Capsule Collider (ou Box Collider) → ajoute-le
Ajuste la taille pour couvrir le sprite du personnage
4. Configurer le NPC trigger (l1)
Clique sur le NPC dans le Hierarchy
En haut de l'Inspector, change le Layer → Interactable
Si Unity demande "Change children too?" → clique Yes, change children
Add Component → cherche Box Collider ou Capsule Collider → ajoute-le (pour le click)
Add Component → cherche Dialogue Proximity Trigger → ajoute-le
Interaction Radius = 3 (ajuste selon le feel)
Demo Scene Trigger = drag le GameObject qui a le script DemoSceneTrigger
Scene Uuid To Launch = copie la valeur de LSDE_SCENES.simpleDialogFlow depuis BlueprintEnums.cs
5. Créer le hint "💬" au-dessus du NPC
Clique-droit sur le NPC dans le Hierarchy → 3D Object → Text - TextMeshPro
Si Unity te demande d'importer TMP Essentials → clique Import
Renomme-le InteractionHint
Positionne-le au-dessus de la tête du NPC (ex: Y = 2)
Dans le composant TextMeshPro :
Text = 💬
Font Size = 5-8 (ajuste au goût)
Alignment = Center
Add Component → cherche Interaction Hint Display → ajoute-le
Hint Text = drag le composant TextMeshPro du même objet
6. Configurer PlayerClickToMoveInput
Sur le joueur, dans le composant Player Click To Move Input :
Interactable Layer Mask = coche Interactable
7. Configurer DemoSceneTrigger
Laisse le champ Auto Launch Scene Uuid vide (le dialogue ne se lance plus automatiquement, c'est le trigger spatial qui s'en charge)

Comment ça marche
Play → le dialogue ne se lance PAS automatiquement
Déplace le joueur vers le NPC → quand il entre dans le rayon, le 💬 apparaît au-dessus du NPC
Clique sur le NPC → le dialogue se lance, le hint disparaît, le joueur ne bouge pas
Le dialogue joue normalement (click pour avancer)
Quand le dialogue se termine → le trigger se réarme, le 💬 réapparaît si le joueur est encore dans la zone