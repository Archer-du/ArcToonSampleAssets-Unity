using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Config
{
    // A single selectable character: the prefab to spawn plus its behavioral states.
    // Animations are driven by the Playables graph, so the prefab only needs an Animator
    // (no AnimatorController is required or used).
    [CreateAssetMenu(
        fileName = "CharacterProfile",
        menuName = "ArcToon Sample/Studio/Character Profile")]
    public class CharacterProfile : ScriptableObject
    {
        [Tooltip("Name shown in the character picker.")]
        public string displayName = "Character";

        [Tooltip("Prefab instantiated when this character is selected. Must contain an Animator.")]
        public GameObject prefab;

        [Tooltip("Height of the camera focus pivot above the character root, used as the default when a preset does not override it.")]
        public float focusHeight = 1f;

        [SerializeField]
        private List<CharacterState> states = new();

        public IReadOnlyList<CharacterState> States => states;

        public int StateCount => states.Count;

        public CharacterState GetState(int index)
        {
            if (index < 0 || index >= states.Count) return null;
            return states[index];
        }

        // Writes the spawn-relative pose of a state (used by the studio's "Save pose to
        // state" authoring action) and marks the asset dirty so the edit survives Play mode.
        public void ApplyStatePose(int stateIndex, Vector3 position, Vector3 rotationEuler)
        {
            var state = GetState(stateIndex);
            if (state == null) return;
            state.localPosition = position;
            state.localRotationEuler = rotationEuler;
            MarkDirty();
        }

        public void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // Flushes in-memory edits to the asset file. Editor-only; a no-op in player builds.
        public void SaveToDisk()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
