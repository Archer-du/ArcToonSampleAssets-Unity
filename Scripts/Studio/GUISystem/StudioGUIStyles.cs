using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.GUISystem
{
    // Lazily built IMGUI styles shared by every studio panel. Styles must be created inside
    // an OnGUI call (GUI.skin is only valid there), so construction is deferred until the
    // first EnsureBuilt() call during layout.
    public class StudioGUIStyles
    {
        // Multiplier applied to every font size and padding so panels stay legible on
        // high-DPI displays, where IMGUI does not scale automatically. Set before the first
        // EnsureBuilt() call (it bakes the scale into the styles).
        public float Scale = 1.5f;

        private bool built;

        public GUIStyle Panel { get; private set; }
        public GUIStyle Title { get; private set; }
        public GUIStyle Header { get; private set; }
        public GUIStyle SubHeader { get; private set; }
        public GUIStyle Label { get; private set; }
        public GUIStyle Hint { get; private set; }
        public GUIStyle Button { get; private set; }
        public GUIStyle ButtonActive { get; private set; }
        public GUIStyle Readout { get; private set; }

        public void EnsureBuilt()
        {
            if (built) return;
            built = true;

            float s = Scale;
            int Pad(int v) => Mathf.RoundToInt(v * s);

            Panel = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(Pad(12), Pad(12), Pad(10), Pad(10)),
                alignment = TextAnchor.UpperLeft
            };

            // The active mode's name, shown once at the top of its panel. Larger and brighter
            // than a section Header to read as the panel title.
            Title = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(19 * s),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.96f, 0.78f) }
            };

            Header = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(15 * s),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            SubHeader = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * s),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.82f, 0.86f, 0.92f) }
            };

            Label = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12 * s),
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            Hint = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12 * s),
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.72f, 0.78f, 0.84f) }
            };

            Readout = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12 * s),
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.65f, 0.85f, 1f) }
            };

            Button = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(12 * s),
                padding = new RectOffset(Pad(10), Pad(10), Pad(6), Pad(6))
            };

            ButtonActive = new GUIStyle(Button)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.55f, 0.85f, 1f) },
                hover = { textColor = new Color(0.55f, 0.85f, 1f) }
            };
        }
    }
}
