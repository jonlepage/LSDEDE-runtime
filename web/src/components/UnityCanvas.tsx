import { Unity } from "react-unity-webgl";
import type { UnityProps } from "react-unity-webgl";

interface UnityCanvasProps {
  unityProvider: UnityProps["unityProvider"];
  isLoaded: boolean;
  loadingProgression: number;
}

export function UnityCanvas({
  unityProvider,
  isLoaded,
  loadingProgression,
}: UnityCanvasProps) {
  return (
    <div className="unity-canvas-container">
      {!isLoaded && (
        <div className="loading-overlay">
          <div className="loading-content">
            <h2>Chargement de Unity...</h2>
            <div className="progress-bar">
              <div
                className="progress-fill"
                style={{ width: `${Math.round(loadingProgression * 100)}%` }}
              />
            </div>
            <p>{Math.round(loadingProgression * 100)}%</p>
          </div>
        </div>
      )}
      <Unity
        unityProvider={unityProvider}
        style={{
          width: "100%",
          height: "100%",
          visibility: isLoaded ? "visible" : "hidden",
        }}
        tabIndex={1}
      />
    </div>
  );
}
