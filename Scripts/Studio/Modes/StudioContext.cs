using ArcToonSampleAssets.Scripts.Studio.Camera;
using ArcToonSampleAssets.Scripts.Studio.Character;
using ArcToonSampleAssets.Scripts.Studio.Config;
using ArcToonSampleAssets.Scripts.Studio.GUISystem;

namespace ArcToonSampleAssets.Scripts.Studio.Modes
{
    // Shared services handed to every operation mode. Constructed and owned by the root
    // CharacterStudioController; modes only read from it and issue requests through it.
    public class StudioContext
    {
        public CharacterStudioController Controller;
        public OrbitCameraRig CameraRig;
        public CharacterInstanceManager Characters;
        public CameraPresetLibrary Presets;
        public CharacterCatalog Catalog;
        public ScreenshotService Screenshot;
        public StudioGUIStyles Styles;

        // Preset viewpoint shared across modes: state-adjust selects it, free-observe seeds
        // from it, and re-entering state-adjust returns to it.
        public int CurrentPresetIndex;

        // Applies a preset viewpoint by index and records it as the current one.
        public void ApplyPreset(int index, bool instant)
        {
            var preset = Presets != null ? Presets.Get(index) : null;
            if (preset == null) return;

            CurrentPresetIndex = index;
            CameraRig.ApplyPreset(preset, instant);
        }
    }
}
