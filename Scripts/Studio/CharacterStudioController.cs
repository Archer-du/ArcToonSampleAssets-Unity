using System.Collections.Generic;
using ArcToonSampleAssets.Scripts.Studio.Camera;
using ArcToonSampleAssets.Scripts.Studio.Character;
using ArcToonSampleAssets.Scripts.Studio.Config;
using ArcToonSampleAssets.Scripts.Studio.GUISystem;
using ArcToonSampleAssets.Scripts.Studio.Modes;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio
{
    // Root of the character studio. Owns the shared context and every operation mode, drives
    // the active mode's input/GUI, and applies the orbit camera each frame. Cross-mode
    // features (mode switching, frame rate, screenshot) live here so each mode stays focused
    // on its own controls.
    //
    // Scene setup: place on a GameObject and assign the target camera, a spawn root, a
    // CharacterCatalog and a CameraPresetLibrary. The camera should be a plain camera with no
    // CinemachineBrain, since the rig writes its transform directly.
    public class CharacterStudioController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private UnityEngine.Camera targetCamera;
        [Tooltip("Parent for the spawned character. Defaults to this transform.")]
        [SerializeField] private Transform characterSpawnRoot;

        [Header("Configuration")]
        [SerializeField] private CharacterCatalog catalog;
        [SerializeField] private CameraPresetLibrary presetLibrary;
        [SerializeField] private OrbitCameraSettings cameraSettings = new();
        [SerializeField] private int initialPresetIndex;

        [Tooltip("Scale applied to all studio GUI text and panels. Raise on high-DPI displays.")]
        [SerializeField] private float guiScale = 1.5f;

        [Header("Global shortcuts")]
        [SerializeField] private KeyCode switchModeKey = KeyCode.Tab;
        [SerializeField] private KeyCode screenshotKey = KeyCode.F2;
        [Tooltip("Shows/hides all studio GUI. Shortcuts keep working while hidden.")]
        [SerializeField] private KeyCode toggleGUIKey = KeyCode.F1;

        private StudioContext context;
        private OrbitCameraRig rig;
        private CharacterInstanceManager characters;
        private ScreenshotService screenshot;
        private FrameRateOverlay frameRate;
        private StudioGUIStyles styles;

        private readonly List<StudioMode> modes = new();
        private int currentModeIndex = -1;

        // Master GUI visibility. When hidden, no panels draw and GUI clicks are impossible,
        // but every keyboard/mouse shortcut still runs through the modes' Tick.
        private bool guiVisible = true;

        public IReadOnlyList<StudioMode> Modes => modes;
        public int CurrentModeIndex => currentModeIndex;
        private StudioMode CurrentMode =>
            currentModeIndex >= 0 && currentModeIndex < modes.Count ? modes[currentModeIndex] : null;

        private void Start()
        {
            if (targetCamera == null) targetCamera = UnityEngine.Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("[CharacterStudio] No target camera assigned and no Camera.main found.");
                enabled = false;
                return;
            }
            if (characterSpawnRoot == null) characterSpawnRoot = transform;

            styles = new StudioGUIStyles { Scale = Mathf.Max(0.5f, guiScale) };
            frameRate = new FrameRateOverlay();
            screenshot = new ScreenshotService(this);
            rig = new OrbitCameraRig(targetCamera, cameraSettings);
            characters = new CharacterInstanceManager(characterSpawnRoot);

            context = new StudioContext
            {
                Controller = this,
                CameraRig = rig,
                Characters = characters,
                Presets = presetLibrary,
                Catalog = catalog,
                Screenshot = screenshot,
                Styles = styles,
                CurrentPresetIndex = initialPresetIndex
            };

            RegisterMode(new StateAdjustMode());
            RegisterMode(new FreeObserveMode());

            var firstProfile = catalog != null ? catalog.FirstProfile() : null;
            if (firstProfile != null) SwitchCharacter(firstProfile);

            SetMode(0);
        }

        private void RegisterMode(StudioMode mode)
        {
            mode.Bind(context);
            modes.Add(mode);
        }

        private void Update()
        {
            frameRate.Tick(Time.unscaledDeltaTime);

            if (Input.GetKeyDown(toggleGUIKey))
            {
                guiVisible = !guiVisible;
            }
            if (modes.Count > 1 && Input.GetKeyDown(switchModeKey))
            {
                SetMode((currentModeIndex + 1) % modes.Count);
            }
            if (Input.GetKeyDown(screenshotKey))
            {
                CaptureScreenshot();
            }

            CurrentMode?.Tick(Time.deltaTime);
            characters.Tick();
        }

        private void LateUpdate()
        {
            // Keep following the current character even if the instance was just respawned.
            rig.SetFocus(characters.FocusRoot);
            rig.Tick(Time.deltaTime);
            rig.Apply();
        }

        private void OnGUI()
        {
            // Hidden entirely by the master toggle, or for the single frame a screenshot fires.
            if (!guiVisible || screenshot.SuppressOverlay) return;

            styles.EnsureBuilt();
            CurrentMode?.OnGUI();
            frameRate.Draw(styles);
            DrawScreenshotToast();
        }

        // --- API used by the modes ---

        public void SetMode(int index)
        {
            if (index < 0 || index >= modes.Count || index == currentModeIndex) return;

            CurrentMode?.OnExit();
            currentModeIndex = index;
            CurrentMode.OnEnter();
        }

        public void SwitchCharacter(CharacterProfile profile)
        {
            characters.SwitchTo(profile);
            rig.SetFocus(characters.FocusRoot);
            context.ApplyPreset(context.CurrentPresetIndex, instant: true);
        }

        public void CaptureScreenshot()
        {
            string label = characters.CurrentProfile != null ? characters.CurrentProfile.displayName : "Studio";
            screenshot.Capture(label);
        }

        private void DrawScreenshotToast()
        {
            if (Time.unscaledTime - screenshot.LastCaptureTime > 2.5f) return;

            float s = styles.Scale;
            var rect = new Rect(10f * s, Screen.height - 40f * s, 600f * s, 30f * s);
            GUI.Label(rect, $"Saved: {screenshot.LastSavedPath}", styles.Hint);
        }

        private void OnDestroy()
        {
            characters?.Cleanup();
        }
    }
}
