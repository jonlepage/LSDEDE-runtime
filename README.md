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
