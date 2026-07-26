using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Config
{
    // A configurable, ordered set of camera presets. Shared across scenes so the same
    // set of viewpoints (Full Body, Face Focus, ...) can be reused by any character.
    [CreateAssetMenu(
        fileName = "CameraPresetLibrary",
        menuName = "ArcToon Sample/Studio/Camera Preset Library")]
    public class CameraPresetLibrary : ScriptableObject
    {
        [SerializeField]
        private List<CameraPreset> presets = new();

        public IReadOnlyList<CameraPreset> Presets => presets;

        public int Count => presets.Count;

        public CameraPreset Get(int index)
        {
            if (index < 0 || index >= presets.Count) return null;
            return presets[index];
        }

        // --- Live editing (Play-mode preset tuning). ---

        // Overwrites an existing preset's viewpoint from a captured live view, keeping its name.
        public void SaveViewpoint(int index, CameraPreset view)
        {
            var target = Get(index);
            if (target == null || view == null) return;

            target.yaw = view.yaw;
            target.pitch = view.pitch;
            target.radius = view.radius;
            target.height = view.height;
            target.fov = view.fov;
            MarkDirty();
        }

        // Appends a new preset from a captured live view and returns its index.
        public int AddViewpoint(string name, CameraPreset view)
        {
            var preset = view != null ? view.Clone() : new CameraPreset();
            preset.name = string.IsNullOrEmpty(name) ? $"Preset {presets.Count + 1}" : name;
            presets.Add(preset);
            MarkDirty();
            return presets.Count - 1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= presets.Count) return;
            presets.RemoveAt(index);
            MarkDirty();
        }

        // Flushes in-memory edits to the asset file. Editor-only; a no-op in player builds.
        public void SaveToDisk()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        // Marks the asset dirty so edits survive exiting Play mode and get written on save.
        private void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
