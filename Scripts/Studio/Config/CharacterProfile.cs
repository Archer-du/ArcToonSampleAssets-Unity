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
    }
}
