using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Config
{
    // A group of characters, typically one source game (Girls' Frontline 2, ZZZ, ...).
    [Serializable]
    public class CharacterGroup
    {
        public string gameName = "Game";

        public List<CharacterProfile> characters = new();
    }

    // Top-level catalog of every selectable character, grouped by game. The studio loads
    // one catalog and flattens it into the character picker.
    [CreateAssetMenu(
        fileName = "CharacterCatalog",
        menuName = "ArcToon Sample/Studio/Character Catalog")]
    public class CharacterCatalog : ScriptableObject
    {
        [SerializeField]
        private List<CharacterGroup> groups = new();

        public IReadOnlyList<CharacterGroup> Groups => groups;

        // Returns the first character in the catalog, or null when the catalog is empty.
        public CharacterProfile FirstProfile()
        {
            foreach (var group in groups)
            {
                if (group.characters == null) continue;
                foreach (var profile in group.characters)
                {
                    if (profile != null) return profile;
                }
            }
            return null;
        }
    }
}
