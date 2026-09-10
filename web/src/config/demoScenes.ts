/** Base URL for blueprint images hosted on the TS demo site */
const BLUEPRINT_BASE =
  "https://jonlepage.github.io/LSDEDE-DEMO-TS/";

export interface BlueprintImages {
  thumbnail: string;
  full: string;
}

export interface DemoScene {
  /** Friendly name sent to Unity via sendMessage (must match C# dictionary keys) */
  sceneName: string;
  /** Display label in sidebar */
  label: string;
  /** Short description of what the demo shows */
  description: string;
  /** Blueprint screenshot paths (thumbnail + full) */
  blueprint: BlueprintImages | null;
}

export const DEMO_SCENES: DemoScene[] = [
  {
    sceneName: "simpleDialogFlow",
    label: "simple-dialog-flow",
    description: "Dialogue linéaire entre 4 personnages",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-1.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-1.webp`,
    },
  },
  {
    sceneName: "multiTracks",
    label: "multi-tracks",
    description: "Pistes de dialogue parallèles simultanées",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-2.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-2.webp`,
    },
  },
  {
    sceneName: "simpleChoices",
    label: "simple-choices",
    description: "Choix du joueur avec embranchements",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-3.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-3.webp`,
    },
  },
  {
    sceneName: "simpleAction",
    label: "simple-action",
    description: "Caméra, mouvement et blocs d'action",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-4.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-4.webp`,
    },
  },
  {
    sceneName: "simpleCondition",
    label: "simple-condition",
    description: "Branchement conditionnel sur l'inventaire",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-5.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-5.webp`,
    },
  },
  {
    sceneName: "conditionDispatch",
    label: "condition-dispatch",
    description: "Dispatcher multi-condition avec pistes parallèles",
    blueprint: {
      thumbnail: `${BLUEPRINT_BASE}lsde-blueprint-sm-6.webp`,
      full: `${BLUEPRINT_BASE}lsde-blueprint-6.webp`,
    },
  },
  {
    // Unity attend ce nom depuis le debut (BuildSceneNameMapping) et les objets de la
    // scene sont filtres dessus — le boss et les carottes ne sont visibles que sous ce
    // drapeau. Sans cette entree la barre laterale ne l'envoie jamais : le dernier
    // bouton lance conditionDispatch, et la demo parait vide.
    // Aucun apercu de blueprint n'est publie pour cette scene (404), d'ou le null.
    sceneName: "advanceFullDemo",
    label: "advance-full-demo",
    description:
      "Démo complète : router, boucle de choix et dictionnaire réservé",
    blueprint: null,
  },
];
