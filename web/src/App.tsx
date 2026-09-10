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
    setLocale,
  } = useUnityInstance();

  const [selectedScene, setSelectedScene] = useState<string | null>(null);
  const [activeLocale, setActiveLocale] = useState("fr");

  const handleSelectScene = useCallback(
    (sceneName: string) => {
      setSelectedScene(sceneName);
      selectScene(sceneName);
    },
    [selectScene],
  );

  const handleSetLocale = useCallback(
    (locale: string) => {
      setActiveLocale(locale);
      setLocale(locale);

      // The engine resolves a line's text when the block is dispatched, so a bubble already
      // on screen keeps the words it was given: switching locale mid-scene changes nothing
      // visible until the next block. Replaying the running demo is what makes the button
      // do what the player expects it to do.
      if (selectedScene !== null) {
        selectScene(selectedScene);
      }
    },
    [setLocale, selectScene, selectedScene],
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
        activeLocale={activeLocale}
        onSelectScene={handleSelectScene}
        onSetLocale={handleSetLocale}
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
