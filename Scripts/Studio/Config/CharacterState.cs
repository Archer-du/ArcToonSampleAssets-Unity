using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Config
{
    // One behavioral state of a character: a looping idle plus a set of one-shot actions.
    // The idle plays continuously; selecting an action plays it once and then fades back
    // to the idle.
    [Serializable]
    public class CharacterState
    {
        public string name = "State";

        [Tooltip("Looping animation played while idle in this state.")]
        public AnimationClip idle;

        [Tooltip("One-shot animations selectable in this state. Each plays once then returns to idle.")]
        public List<AnimationClip> actions = new();

        public bool HasIdle => idle != null;

        public int ActionCount => actions?.Count ?? 0;

        public AnimationClip GetAction(int index)
        {
            if (actions == null || index < 0 || index >= actions.Count) return null;
            return actions[index];
        }
    }
}
