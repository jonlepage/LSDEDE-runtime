import { useState } from "react";
import type { BlueprintImages } from "../config/demoScenes";

interface BlueprintPreviewProps {
  blueprint: BlueprintImages | null;
}

export function BlueprintPreview({ blueprint }: BlueprintPreviewProps) {
  const [isHovered, setIsHovered] = useState(false);

  if (!blueprint) return null;

  return (
    <button
      className="blueprint-button"
      title="View blueprint screenshot"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={() => window.open(blueprint.full, "_blank", "noopener,noreferrer")}
    >
      🗺️
      {isHovered && (
        <div className="blueprint-tooltip">
          <img src={blueprint.thumbnail} alt="Blueprint preview" />
          <span>Click to open full blueprint</span>
        </div>
      )}
    </button>
  );
}
