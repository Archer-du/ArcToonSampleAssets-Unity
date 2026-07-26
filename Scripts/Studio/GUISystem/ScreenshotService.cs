using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.GUISystem
{
    // Captures the rendered frame to a PNG next to the project (<project>/Screenshots).
    // The studio GUI is suppressed for the capture frame so screenshots show only the
    // rendered scene. Requires a MonoBehaviour host to run the end-of-frame coroutine.
    public class ScreenshotService
    {
        private readonly MonoBehaviour host;

        // While true, the studio must skip drawing its IMGUI overlay.
        public bool SuppressOverlay { get; private set; }

        public string LastSavedPath { get; private set; }

        // Real-time timestamp (unscaled) when the last capture completed, for a toast.
        public float LastCaptureTime { get; private set; } = -100f;

        private bool capturing;

        public ScreenshotService(MonoBehaviour host)
        {
            this.host = host;
        }

        public void Capture(string label)
        {
            if (capturing) return;
            host.StartCoroutine(CaptureRoutine(label));
        }

        private IEnumerator CaptureRoutine(string label)
        {
            capturing = true;
            SuppressOverlay = true;

            // Let the suppressed-overlay state take effect, then grab the finished frame.
            yield return new WaitForEndOfFrame();

            string dir = Path.Combine(Application.dataPath, "..", "Screenshots");
            dir = Path.GetFullPath(dir);
            Directory.CreateDirectory(dir);

            string safeLabel = string.IsNullOrEmpty(label) ? "Studio" : Sanitize(label);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(dir, $"{safeLabel}_{stamp}.png");

            ScreenCapture.CaptureScreenshot(path);

            LastSavedPath = path;
            LastCaptureTime = Time.unscaledTime;

            // Restore the overlay on the next frame so it is absent from the captured image.
            yield return null;
            SuppressOverlay = false;
            capturing = false;

            Debug.Log($"[CharacterStudio] Screenshot saved: {path}");
        }

        private static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s.Replace(' ', '_');
        }
    }
}
