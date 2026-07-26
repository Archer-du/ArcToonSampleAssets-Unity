using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Modes
{
    // Camera-only mode. The cursor is locked and mouse movement orbits the camera; scroll
    // zooms, Q/E raise/lower the pivot, and the arrow keys widen/narrow the FOV. All limits
    // are shown in the panel. Mode switch and screenshot are global shortcuts (Tab / F2).
    public class FreeObserveMode : StudioMode
    {
        public override string DisplayName => "Free Observe";

        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void Tick(float deltaTime)
        {
            var rig = Context.CameraRig;
            var s = rig.Settings;

            // Mouse look (cursor locked): X orbits horizontally, Y orbits vertically.
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (mouseX != 0f || mouseY != 0f)
            {
                rig.RotateBy(mouseX * s.rotateSpeed, -mouseY * s.rotateSpeed);
            }

            // Scroll wheel zooms (scroll up = closer).
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                rig.Zoom(-scroll * s.zoomSpeed);
            }

            // Q/E adjust pivot height.
            if (Input.GetKey(KeyCode.E)) rig.AdjustHeight(deltaTime * s.heightSpeed);
            if (Input.GetKey(KeyCode.Q)) rig.AdjustHeight(-deltaTime * s.heightSpeed);

            // Up/Down arrows adjust field of view.
            if (Input.GetKey(KeyCode.UpArrow)) rig.AdjustFov(deltaTime * s.fovSpeed);
            if (Input.GetKey(KeyCode.DownArrow)) rig.AdjustFov(-deltaTime * s.fovSpeed);
        }

        public override void OnGUI()
        {
            var styles = Context.Styles;
            var rig = Context.CameraRig;
            var s = rig.Settings;
            float g = styles.Scale;

            GUILayout.BeginArea(new Rect(10f * g, 10f * g, 300f * g, 300f * g), GUIContent.none, styles.Panel);
            GUILayout.Label("Free Observe", styles.Title);
            GUILayout.Space(12f);

            GUILayout.Label("Mouse: orbit camera", styles.Hint);
            GUILayout.Label("Scroll: distance   Q/E: height   Up/Down: FOV", styles.Hint);
            GUILayout.Space(8f);

            DrawReadout("Distance", rig.Radius, s.radiusMin, s.radiusMax);
            DrawReadout("Height", rig.Height, s.heightMin, s.heightMax);
            DrawReadout("FOV", rig.Fov, s.fovMin, s.fovMax);

            GUILayout.Space(10f);
            GUILayout.Label("Tab: switch to State Adjust", styles.Hint);
            GUILayout.Label("F2: screenshot   F1: hide GUI", styles.Hint);
            GUILayout.EndArea();
        }

        private void DrawReadout(string label, float value, float min, float max)
        {
            var styles = Context.Styles;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value:0.00}", styles.Label, GUILayout.Width(130f * styles.Scale));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"[{min:0.#} .. {max:0.#}]", styles.Readout);
            GUILayout.EndHorizontal();
        }
    }
}
