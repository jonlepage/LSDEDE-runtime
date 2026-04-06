import { useUnityContext } from "react-unity-webgl";
import { useState, useCallback, useEffect } from "react";

/**
 * Custom hook that wraps react-unity-webgl and provides a typed API
 * for communicating with the LSDEDE Unity WebGL build.
 *
 * Handles:
 * - Unity context initialization (loader, framework, data, wasm)
 * - Scene selection via sendMessage to WebGlSceneController
 * - Event listeners for Unity→React notifications (ready, scene started/completed)
 */
export function useUnityInstance() {
  const {
    unityProvider,
    sendMessage,
    addEventListener,
    removeEventListener,
    isLoaded,
    loadingProgression,
  } = useUnityContext({
    loaderUrl: "unity-build/Build/unity-build.loader.js",
    dataUrl: "unity-build/Build/unity-build.data.unityweb",
    frameworkUrl: "unity-build/Build/unity-build.framework.js.unityweb",
    codeUrl: "unity-build/Build/unity-build.wasm.unityweb",
  });

  const [isUnityReady, setIsUnityReady] = useState(false);
  const [activeScene, setActiveScene] = useState<string | null>(null);
  const [isSceneComplete, setIsSceneComplete] = useState(false);

  useEffect(() => {
    const handleUnityReady = () => setIsUnityReady(true);

    const handleSceneStarted = (sceneName: unknown) => {
      if (typeof sceneName === "string") {
        setActiveScene(sceneName);
        setIsSceneComplete(false);
      }
    };

    const handleSceneCompleted = (_sceneName: unknown) => {
      setIsSceneComplete(true);
    };

    addEventListener("UnityReady", handleUnityReady);
    addEventListener("SceneStarted", handleSceneStarted);
    addEventListener("SceneCompleted", handleSceneCompleted);

    return () => {
      removeEventListener("UnityReady", handleUnityReady);
      removeEventListener("SceneStarted", handleSceneStarted);
      removeEventListener("SceneCompleted", handleSceneCompleted);
    };
  }, [addEventListener, removeEventListener]);

  const selectScene = useCallback(
    (sceneName: string) => {
      if (isLoaded) {
        sendMessage("WebGlSceneController", "SelectScene", sceneName);
      }
    },
    [sendMessage, isLoaded],
  );

  return {
    unityProvider,
    isLoaded,
    loadingProgression,
    isUnityReady,
    activeScene,
    isSceneComplete,
    selectScene,
  };
}
