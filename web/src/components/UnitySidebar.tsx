import { DEMO_SCENES } from "../config/demoScenes";

interface UnitySidebarProps {
  activeScene: string | null;
  isUnityReady: boolean;
  isSceneComplete: boolean;
  onSelectScene: (sceneName: string) => void;
}

const EXTERNAL_LINKS = [
  {
    label: "LSDE Documentation",
    icon: "\u{1F4D6}",
    href: "https://jonlepage.github.io/LS-Dialog-Editor-Engine/",
  },
  {
    label: "Runtime Repository",
    icon: "\u{2039}\u{203A}",
    href: "https://github.com/jonlepage/LSDEDE-runtime",
  },
  {
    label: "Playground Repository",
    icon: "\u{1F3AE}",
    href: "https://github.com/jonlepage/LSDEDE-DEMO-TS",
  },
  {
    label: "LSDE Official Website",
    icon: "\u{1F310}",
    href: "https://lepasoft.com/en/software/ls-dialog-editor",
  },
];

export function UnitySidebar({
  activeScene,
  isUnityReady,
  isSceneComplete,
  onSelectScene,
}: UnitySidebarProps) {
  return (
    <aside className="demo-sidebar">
      {/* Header */}
      <div className="sidebar-header">
        <a
          className="sidebar-logo-link"
          href="https://lepasoft.com/en/software/ls-dialog-editor"
          target="_blank"
          rel="noopener noreferrer"
        >
          <img
            className="sidebar-logo"
            src="https://jonlepage.github.io/LSDEDE-DEMO-TS/lsde-logo.webp"
            alt="LSDE Logo"
          />
          <span className="sidebar-brand">LSDE Playground</span>
        </a>
      </div>

      {/* External links */}
      <div className="sidebar-links">
        {EXTERNAL_LINKS.map((link) => (
          <a
            key={link.href}
            href={link.href}
            target="_blank"
            rel="noopener noreferrer"
          >
            <span className="sidebar-link-icon">{link.icon}</span>
            {link.label}
          </a>
        ))}
      </div>

      {/* Demo list */}
      <div className="sidebar-demos">
        <h2 className="sidebar-demos-title">LSDE Demos</h2>
        <ul className="scene-list">
          {DEMO_SCENES.map((scene) => (
            <li key={scene.sceneName}>
              <button
                className={`demo-scene-button ${activeScene === scene.sceneName ? "active" : ""}`}
                disabled={!isUnityReady}
                onClick={() => onSelectScene(scene.sceneName)}
              >
                {scene.label}
                {activeScene === scene.sceneName && isSceneComplete && " ✓"}
              </button>
            </li>
          ))}
        </ul>
      </div>

      {!isUnityReady && (
        <div className="sidebar-status">
          <p>En attente du chargement Unity...</p>
        </div>
      )}
    </aside>
  );
}
