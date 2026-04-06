import { useCallback, useEffect, useMemo, useState } from "react";
import { useUnityInstance } from "./hooks/useUnityInstance";
import { UnitySidebar } from "./components/UnitySidebar";
import { UnityCanvas } from "./components/UnityCanvas";
import { BlueprintPreview } from "./components/BlueprintPreview";
import { DEMO_SCENES } from "./config/demoScenes";

export default function App() {
  const {
    unityProvider,
    isLoaded,
    loadingProgression,
    isUnityReady,
    activeScene,
    isSceneComplete,
    selectScene,
  } = useUnityInstance();

  const [selectedScene, setSelectedScene] = useState<string | null>(null);

  const handleSelectScene = useCallback(
    (sceneName: string) => {
      setSelectedScene(sceneName);
      selectScene(sceneName);
    },
    [selectScene],
  );

  // When Unity is ready, select the first scene
  useEffect(() => {
    if (isUnityReady && selectedScene === null) {
      handleSelectScene(DEMO_SCENES[0]!.sceneName);
    }
  }, [isUnityReady, selectedScene, handleSelectScene]);

  const displayScene = activeScene ?? selectedScene;

  const activeBlueprint = useMemo(() => {
    const scene = DEMO_SCENES.find((s) => s.sceneName === displayScene);
    return scene?.blueprint ?? null;
  }, [displayScene]);

  return (
    <div className="app-layout">
      <UnitySidebar
        activeScene={displayScene}
        isUnityReady={isUnityReady}
        isSceneComplete={isSceneComplete}
        onSelectScene={handleSelectScene}
      />
      <main className="unity-main">
        <BlueprintPreview blueprint={activeBlueprint} />
        <UnityCanvas
          unityProvider={unityProvider}
          isLoaded={isLoaded}
          loadingProgression={loadingProgression}
        />
      </main>
    </div>
  );
}
