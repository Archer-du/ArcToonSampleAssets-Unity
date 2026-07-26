using ArcToonSampleAssets.Scripts.Studio.Config;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Modes
{
    // Default mode for posing the character and dialing in preset viewpoints. Number keys
    // (or panel buttons) select a preset viewpoint, left-drag rotates the character, and the
    // animation sub-panel drives states and one-shot action clips. Space pauses animation.
    public class StateAdjustMode : StudioMode
    {
        private const float BasePanelWidth = 340f;
        private const float CharacterRotateSpeed = 6f;

        // Panel geometry scales with the GUI scale so the panel stays proportional to its
        // (scaled) text on high-DPI displays. PanelRight is in screen pixels and is also the
        // drag-exclusion boundary used in Tick.
        private float PanelWidth => BasePanelWidth * Context.Styles.Scale;
        private float PanelRight => 10f * Context.Styles.Scale + PanelWidth;

        private Vector2 scroll;
        private bool draggingCharacter;

        // Collapsed by default: a state may hold many action clips, and the foldout header
        // shows the clip currently playing so the list can stay closed most of the time.
        private bool actionsExpanded;

        // When on, the preset editor sub-panel is shown and the camera can be tuned live
        // (right-drag orbit, scroll zoom, Q/E height, arrows FOV) to author preset viewpoints.
        private bool editingPresets;

        public override string DisplayName => "State Adjust";

        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Return to the shared preset viewpoint when re-entering this mode.
            Context.ApplyPreset(Context.CurrentPresetIndex, instant: false);
        }

        public override void Tick(float deltaTime)
        {
            HandlePresetKeys();
            HandleCharacterDrag();
            if (editingPresets) HandleCameraTuning(deltaTime);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Context.Characters.SetPaused(!Context.Characters.IsPaused);
            }
        }

        private void HandlePresetKeys()
        {
            int count = Context.GetActivePresetLibrary()?.Count ?? 0;
            int max = Mathf.Min(count, 9);
            for (int i = 0; i < max; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Context.ApplyPreset(i, instant: false);
                    break;
                }
            }
        }

        private void HandleCharacterDrag()
        {
            // Start a drag only when the press begins outside the panel, so clicking buttons
            // never spins the character.
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > PanelRight)
            {
                draggingCharacter = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                draggingCharacter = false;
            }

            if (draggingCharacter && Input.GetMouseButton(0))
            {
                float dx = Input.GetAxis("Mouse X");
                if (dx != 0f) Context.Characters.RotateCharacter(-dx * CharacterRotateSpeed);
            }
        }

        // Free camera control for authoring presets. Uses right-drag for orbit so it never
        // clashes with left-drag character rotation.
        private void HandleCameraTuning(float deltaTime)
        {
            var rig = Context.CameraRig;
            var s = rig.Settings;

            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                if (mouseX != 0f || mouseY != 0f)
                {
                    rig.RotateBy(mouseX * s.rotateSpeed, -mouseY * s.rotateSpeed);
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f) rig.Zoom(-scroll * s.zoomSpeed);

            if (Input.GetKey(KeyCode.E)) rig.AdjustHeight(deltaTime * s.heightSpeed);
            if (Input.GetKey(KeyCode.Q)) rig.AdjustHeight(-deltaTime * s.heightSpeed);

            if (Input.GetKey(KeyCode.UpArrow)) rig.AdjustFov(deltaTime * s.fovSpeed);
            if (Input.GetKey(KeyCode.DownArrow)) rig.AdjustFov(-deltaTime * s.fovSpeed);
        }

        public override void OnGUI()
        {
            var styles = Context.Styles;
            var rect = new Rect(10f * styles.Scale, 10f * styles.Scale, PanelWidth, Screen.height - 20f * styles.Scale);

            GUILayout.BeginArea(rect, GUIContent.none, styles.Panel);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("State Adjust", styles.Title);
            GUILayout.Space(12f);

            DrawStateSelector();
            DrawPresets();
            DrawPresetEditor();
            DrawCharacterPicker();
            DrawActionsPanel();
            DrawFooter();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawPresets()
        {
            var styles = Context.Styles;
            var presets = Context.GetActivePresetLibrary();
            GUILayout.Label("Viewpoint Presets", styles.Header);
            if (presets == null || presets.Count == 0)
            {
                GUILayout.Label("(no preset library assigned)", styles.Hint);
                return;
            }

            const int perRow = 3;
            for (int i = 0; i < presets.Count; i++)
            {
                if (i % perRow == 0) GUILayout.BeginHorizontal();

                var preset = presets.Get(i);
                bool active = i == Context.CurrentPresetIndex;
                string label = $"{i + 1}. {preset.name}";
                if (GUILayout.Button(label, active ? styles.ButtonActive : styles.Button))
                {
                    Context.ApplyPreset(i, instant: false);
                }

                if (i % perRow == perRow - 1 || i == presets.Count - 1) GUILayout.EndHorizontal();
            }
            GUILayout.Space(8f);
        }

        // Live authoring: tune the camera (sliders or right-drag/scroll/keys) while watching,
        // then save the current view back to the active library and flush it to the asset. The
        // active library is the one resolved for the current state (the state's own library,
        // or the controller default when none is assigned); a parallel "Save pose to state"
        // section persists the character's spawn-relative transform onto the state.
        private void DrawPresetEditor()
        {
            var styles = Context.Styles;
            var presets = Context.GetActivePresetLibrary();
            if (presets == null) return;

            string toggleLabel = editingPresets ? "Editing (click to finish)" : "Edit viewpoints & pose...";
            if (GUILayout.Button(toggleLabel, editingPresets ? styles.ButtonActive : styles.Button))
            {
                editingPresets = !editingPresets;
            }
            if (!editingPresets)
            {
                GUILayout.Space(8f);
                return;
            }

            var rig = Context.CameraRig;
            var s = rig.Settings;
            var characters = Context.Characters;
            var profile = characters.CurrentProfile;
            var state = profile != null ? profile.GetState(characters.CurrentStateIndex) : null;

            // Warn when edits land on the shared default library: every state without its own
            // library shares that asset, so a save here propagates to all of them.
            if (state == null || state.presetLibrary == null)
            {
                GUILayout.Label("(shared default library — edits affect all states using it)", styles.Hint);
            }

            GUILayout.Label("Camera: right-drag orbit, scroll zoom, Q/E height, arrows FOV", styles.Hint);

            // Sliders drive the live camera directly; a slider only writes back when the user
            // actually moves it, so selecting a preset still blends normally.
            DrawParamSlider("Yaw", rig.Yaw, 0f, 360f, rig.SetYaw);
            DrawParamSlider("Pitch", rig.Pitch, s.pitchMin, s.pitchMax, rig.SetPitch);
            DrawParamSlider("Distance", rig.Radius, s.radiusMin, s.radiusMax, rig.SetRadius);
            DrawParamSlider("Height", rig.Height, s.heightMin, s.heightMax, rig.SetHeight);
            DrawParamSlider("FOV", rig.Fov, s.fovMin, s.fovMax, rig.SetFov);

            GUILayout.Space(4f);
            int index = Context.CurrentPresetIndex;
            var current = presets.Get(index);

            GUILayout.BeginHorizontal();
            if (current != null && GUILayout.Button($"Save to \"{current.name}\"", styles.Button))
            {
                presets.SaveViewpoint(index, rig.CaptureCurrent());
            }
            if (GUILayout.Button("+ New from view", styles.Button))
            {
                Context.CurrentPresetIndex = presets.AddViewpoint(null, rig.CaptureCurrent());
            }
            GUILayout.EndHorizontal();

            if (current != null && presets.Count > 1 &&
                GUILayout.Button($"Delete \"{current.name}\"", styles.Button))
            {
                presets.RemoveAt(index);
                Context.CurrentPresetIndex = Mathf.Clamp(index, 0, presets.Count - 1);
                Context.ApplyPreset(Context.CurrentPresetIndex, instant: true);
            }

            if (GUILayout.Button("Save library to disk", styles.Button))
            {
                presets.SaveToDisk();
            }

            GUILayout.Space(6f);
            GUILayout.Label("Character pose", styles.SubHeader);
            var focus = characters.FocusRoot;
            if (focus != null)
            {
                // Sliders write the live instance transform only; persist with "Save current
                // pose to state" below, mirroring how the camera sliders edit the live rig
                // until "Save to preset" is pressed. Unsaved edits are discarded on state switch.
                DrawParamSlider("Pos X", focus.localPosition.x, -10f, 10f, v => ApplyPoseAxis(focus, 0, v));
                DrawParamSlider("Pos Y", focus.localPosition.y, -10f, 10f, v => ApplyPoseAxis(focus, 1, v));
                DrawParamSlider("Pos Z", focus.localPosition.z, -10f, 10f, v => ApplyPoseAxis(focus, 2, v));
                DrawParamSlider("Rot Y", focus.localRotation.eulerAngles.y, 0f, 360f, v => ApplyPoseYaw(focus, v));
            }
            if (GUILayout.Button("Save current pose to state", styles.Button) && focus != null && profile != null)
            {
                profile.ApplyStatePose(characters.CurrentStateIndex, focus.localPosition, focus.localRotation.eulerAngles);
            }
            if (profile != null && GUILayout.Button("Save profile to disk", styles.Button))
            {
                profile.SaveToDisk();
            }
            GUILayout.Space(8f);
        }

        // Pose slider helpers: write the live instance transform only. They do not persist;
        // "Save current pose to state" captures the live transform into the state asset.
        private void ApplyPoseAxis(Transform focus, int axis, float value)
        {
            var p = focus.localPosition;
            p[axis] = value;
            focus.localPosition = p;
        }

        private void ApplyPoseYaw(Transform focus, float yaw)
        {
            var euler = focus.localRotation.eulerAngles;
            euler.y = yaw;
            focus.localRotation = Quaternion.Euler(euler);
        }

        // Draws a labelled slider; writes back through setter only when the user moves it.
        private void DrawParamSlider(string label, float value, float min, float max, System.Action<float> setter)
        {
            var styles = Context.Styles;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, styles.Label, GUILayout.Width(70f * styles.Scale));
            float result = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.Label($"{value:0.0}", styles.Readout, GUILayout.Width(52f * styles.Scale));
            GUILayout.EndHorizontal();

            if (result != value) setter(result);
        }

        private void DrawCharacterPicker()
        {
            var styles = Context.Styles;
            var catalog = Context.Catalog;
            GUILayout.Label("Character", styles.Header);
            if (catalog == null || catalog.Groups.Count == 0)
            {
                GUILayout.Label("(no catalog assigned)", styles.Hint);
                return;
            }

            var current = Context.Characters.CurrentProfile;
            foreach (var group in catalog.Groups)
            {
                if (group == null || group.characters == null || group.characters.Count == 0) continue;

                GUILayout.Label(group.gameName, styles.SubHeader);
                foreach (var profile in group.characters)
                {
                    if (profile == null) continue;
                    bool active = profile == current;
                    if (GUILayout.Button(profile.displayName, active ? styles.ButtonActive : styles.Button))
                    {
                        Context.Controller.SwitchCharacter(profile);
                    }
                }
            }
            GUILayout.Space(8f);
        }

        // Top-level section: the current state drives every other section (presets, pose,
        // actions), so it sits at the top of the panel.
        private void DrawStateSelector()
        {
            var styles = Context.Styles;
            var characters = Context.Characters;
            var profile = characters.CurrentProfile;

            GUILayout.Label("State", styles.Header);
            if (profile == null || profile.StateCount == 0)
            {
                GUILayout.Label("(character has no states)", styles.Hint);
                return;
            }

            const int perRow = 3;
            for (int i = 0; i < profile.StateCount; i++)
            {
                if (i % perRow == 0) GUILayout.BeginHorizontal();

                bool active = i == characters.CurrentStateIndex;
                var state = profile.GetState(i);
                if (GUILayout.Button(state.name, active ? styles.ButtonActive : styles.Button))
                {
                    characters.SetState(i);
                    Context.SyncToCurrentState(instant: false);
                }

                if (i % perRow == perRow - 1 || i == profile.StateCount - 1) GUILayout.EndHorizontal();
            }
            GUILayout.Space(8f);
        }

        private void DrawActionsPanel()
        {
            var styles = Context.Styles;
            var characters = Context.Characters;
            var profile = characters.CurrentProfile;

            GUILayout.Label("Animation", styles.Header);
            if (profile == null || profile.StateCount == 0)
            {
                GUILayout.Label("(character has no states)", styles.Hint);
                return;
            }

            // Available text width inside the scroll view: panel width minus its padding and
            // the vertical scrollbar. Long clip names are clamped to this so a single wide
            // button cannot stretch the whole scroll content and misalign the other rows.
            float contentWidth = PanelWidth - 2f * Mathf.RoundToInt(12f * styles.Scale) - 16f;

            // Actions of the current state: play once then return to idle. The list is
            // collapsible; its header shows the clip currently playing (or the idle when no
            // one-shot is active) so a long clip list can stay folded away.
            var currentState = profile.GetState(characters.CurrentStateIndex);
            GUILayout.Space(4f);

            var playingClip = characters.CurrentAction ?? characters.CurrentIdle;
            string playingName = playingClip != null ? playingClip.name : "(none)";
            string foldMark = actionsExpanded ? "[-]" : "[+]";
            if (GUILayout.Button(BuildClampedLabel($"{foldMark} Actions: ", playingName, styles.Button, contentWidth), styles.Button))
            {
                actionsExpanded = !actionsExpanded;
            }

            if (actionsExpanded)
            {
                if (currentState == null || currentState.ActionCount == 0)
                {
                    GUILayout.Label("(no actions in this state)", styles.Hint);
                }
                else
                {
                    for (int i = 0; i < currentState.ActionCount; i++)
                    {
                        var clip = currentState.GetAction(i);
                        string name = clip != null ? clip.name : "(missing)";
                        if (GUILayout.Button(BuildClampedLabel(">  ", name, styles.Button, contentWidth), styles.Button))
                        {
                            characters.PlayAction(i);
                        }
                    }
                }
            }

            GUILayout.Space(4f);
            string pauseLabel = characters.IsPaused ? "Resume (Space)" : "Pause (Space)";
            if (GUILayout.Button(pauseLabel, styles.Button))
            {
                characters.SetPaused(!characters.IsPaused);
            }
            GUILayout.Space(8f);
        }

        // Builds "prefix + name" as a button label, truncating name with "..." when the full
        // label would exceed maxWidth (measured with the style's font). The full name is kept
        // as the tooltip so it is still reachable on hover.
        private static GUIContent BuildClampedLabel(string prefix, string name, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(name)) return new GUIContent(prefix, name);

            var content = new GUIContent(prefix + name, name);
            if (style.CalcSize(content).x <= maxWidth) return content;

            float budget = maxWidth - style.CalcSize(new GUIContent(prefix)).x;
            const string ellipsis = "...";
            string best = ellipsis;
            for (int len = name.Length - 1; len > 0; len--)
            {
                string candidate = name.Substring(0, len) + ellipsis;
                if (style.CalcSize(new GUIContent(candidate)).x <= budget)
                {
                    best = candidate;
                    break;
                }
            }
            content.text = prefix + best;
            return content;
        }

        private void DrawFooter()
        {
            var styles = Context.Styles;
            GUILayout.Label("Left-drag: rotate character", styles.Hint);
            GUILayout.Space(4f);

            // Switch to any other registered mode.
            var controller = Context.Controller;
            var modes = controller.Modes;
            for (int i = 0; i < modes.Count; i++)
            {
                if (i == controller.CurrentModeIndex) continue;
                if (GUILayout.Button($"Switch to {modes[i].DisplayName} (Tab)", styles.Button))
                {
                    controller.SetMode(i);
                }
            }

            if (GUILayout.Button("Screenshot (F2)", styles.Button))
            {
                controller.CaptureScreenshot();
            }

            GUILayout.Space(4f);
            GUILayout.Label("F1: hide GUI (shortcuts still work)", styles.Hint);
        }
    }
}
