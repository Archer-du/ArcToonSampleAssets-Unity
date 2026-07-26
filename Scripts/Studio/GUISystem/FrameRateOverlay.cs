using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.GUISystem
{
    // Smoothed frame-rate readout drawn in the top-right corner, independent of the active
    // operation mode.
    public class FrameRateOverlay
    {
        private float smoothedDeltaTime;

        public void Tick(float unscaledDeltaTime)
        {
            // Exponential smoothing so the number is readable rather than jittery.
            smoothedDeltaTime += (unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
        }

        public void Draw(StudioGUIStyles styles)
        {
            float fps = smoothedDeltaTime > 0f ? 1f / smoothedDeltaTime : 0f;
            float ms = smoothedDeltaTime * 1000f;
            string text = $"{fps:0.} FPS  ({ms:0.0} ms)";

            const float width = 150f;
            const float height = 24f;
            float s = styles.Scale;
            float w = width * s;
            float h = height * s;
            var rect = new Rect(Screen.width - w - 10f * s, 10f * s, w, h);

            GUI.Box(rect, GUIContent.none, styles.Panel);
            GUI.Label(rect, text, styles.Readout);
        }
    }
}
