using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Character
{
    // Toggles GameObject visibility among a set of manually-referenced, mutually exclusive options.
    // Only the option at Selected Index is active after each apply; Selected Index = -1 deactivates
    // every option (e.g. "no hat"). The selection is applied on enable and on every inspector change.
    [ExecuteAlways]
    public class ExclusiveObjectToggle : MonoBehaviour
    {
        [Tooltip("Mutually exclusive candidate GameObjects. Only the one at Selected Index is shown.")]
        [SerializeField] private List<GameObject> options = new();

        [Tooltip("Index of the option to show. -1 hides all options (None).")]
        [SerializeField] private int selectedIndex = -1;

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            ClampSelectedIndex();
            Apply();
        }

        // Applies the current selection to GameObject active state. Public so the inspector or an
        // initial setup step can force a refresh after external active-state edits. It never changes
        // the selection, so it is not a runtime switch API.
        public void Apply()
        {
            if (options == null) return;
            for (int i = 0; i < options.Count; i++)
            {
                GameObject option = options[i];
                if (option == null) continue;
                option.SetActive(i == selectedIndex);
            }
        }

        private void ClampSelectedIndex()
        {
            if (selectedIndex < -1) selectedIndex = -1;
            if (options != null && selectedIndex >= options.Count)
                selectedIndex = options.Count - 1;
        }
    }
}
