using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Character
{
    // Keeps the active state of every member GameObject in sync: toggling any member's active state
    // (e.g. via the hierarchy/inspector checkbox) makes every other member follow. Runs in edit mode
    // via ExecuteAlways. Place on a GameObject that stays active and is NOT listed as a member --
    // if a member disables this component's own GameObject, Update stops and the group can no longer
    // be re-enabled by this component.
    [ExecuteAlways]
    public class LinkedActiveGroup : MonoBehaviour
    {
        [Tooltip("GameObjects whose active state is kept in sync. Do not include this component's own GameObject.")]
        [SerializeField] private List<GameObject> members = new();

        private readonly List<bool> lastStates = new();
        private bool dirty = true;

        private void OnEnable()
        {
            dirty = true;
        }

        private void OnValidate()
        {
            dirty = true;
            WarnIfSelfInMembers();
        }

        private void Update()
        {
            if (members == null) return;
            SyncLastStatesLength();

            if (dirty)
            {
                RecordCurrentStates();
                dirty = false;
                return;
            }

            for (int i = 0; i < members.Count; i++)
            {
                GameObject member = members[i];
                if (member == null) continue;
                if (member.activeSelf != lastStates[i])
                {
                    SetAll(member.activeSelf);
                    RecordCurrentStates();
                    return;
                }
            }
        }

        private void SetAll(bool target)
        {
            for (int i = 0; i < members.Count; i++)
            {
                GameObject member = members[i];
                if (member == null) continue;
                member.SetActive(target);
            }
        }

        private void SyncLastStatesLength()
        {
            while (lastStates.Count < members.Count) lastStates.Add(true);
            while (lastStates.Count > members.Count) lastStates.RemoveAt(lastStates.Count - 1);
        }

        private void RecordCurrentStates()
        {
            for (int i = 0; i < members.Count; i++)
            {
                lastStates[i] = members[i] != null && members[i].activeSelf;
            }
        }

        private void WarnIfSelfInMembers()
        {
            if (members == null) return;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == gameObject)
                {
                    Debug.LogWarning($"[{nameof(LinkedActiveGroup)}] This component's own GameObject should not be a member; disabling it would stop the component.", this);
                    return;
                }
            }
        }
    }
}
