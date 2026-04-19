import posthog from "posthog-js";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import "./App.css";

posthog.init("phc_ytWJv3iJS9BKoXoFAac8P7MGGfUppdxxojecAXsr8aFY", {
  api_host: "https://e.lepasoft.com",
  ui_host: "https://us.posthog.com",
  autocapture: true,
  capture_pageview: true,
  capture_pageleave: true,
  disable_session_recording: false,
  person_profiles: 'always',
  session_recording: {
    maskAllInputs: true,
    maskTextSelector: "",
    recordCrossOriginIframes: false,
    captureCanvas: {
      canvasFps: 4,
      canvasQuality: "0.4",
    },
  },
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
