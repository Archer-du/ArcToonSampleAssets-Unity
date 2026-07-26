using ArcToonSampleAssets.Scripts.Studio.Camera;
using ArcToonSampleAssets.Scripts.Studio.Character;
using ArcToonSampleAssets.Scripts.Studio.Config;
using ArcToonSampleAssets.Scripts.Studio.GUISystem;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Modes
{
    // Shared services handed to every operation mode. Constructed and owned by the root
    // CharacterStudioController; modes only read from it and issue requests through it.
    public class StudioContext
    {
        public CharacterStudioController Controller;
        public OrbitCameraRig CameraRig;
        public CharacterInstanceManager Characters;
        public CharacterCatalog Catalog;
        public ScreenshotService Screenshot;
        public StudioGUIStyles Styles;

        // Fallback library used when the current state has no library of its own assigned.
        public CameraPresetLibrary DefaultPresetLibrary;

        // Index within the current state's active preset library. Re-clamped to range by
        // SyncToCurrentState whenever the active library changes.
        public int CurrentPresetIndex;

        // Resolves the preset library governing the current state: the state's own library,
        // or the controller default when the state has none.
        public CameraPresetLibrary GetActivePresetLibrary()
        {
            var profile = Characters != null ? Characters.CurrentProfile : null;
            var state = profile != null ? profile.GetState(Characters.CurrentStateIndex) : null;
            return state != null && state.presetLibrary != null ? state.presetLibrary : DefaultPresetLibrary;
        }

        // Applies a preset viewpoint by index from the active library and records it as current.
        public void ApplyPreset(int index, bool instant)
        {
            var library = GetActivePresetLibrary();
            var preset = library != null ? library.Get(index) : null;
            if (preset == null) return;

            CurrentPresetIndex = index;
            CameraRig.ApplyPreset(preset, instant);
        }

        // Re-binds to the current state after a state or character switch: clamps the preset
        // index into range and applies the resulting viewpoint. The camera pivot itself is
        // fixed to the spawn root and does not move on a switch.
        public void SyncToCurrentState(bool instant)
        {
            int count = GetActivePresetLibrary()?.Count ?? 0;
            CurrentPresetIndex = Mathf.Clamp(CurrentPresetIndex, 0, Mathf.Max(0, count - 1));
            if (count > 0) ApplyPreset(CurrentPresetIndex, instant);
        }
    }
}
