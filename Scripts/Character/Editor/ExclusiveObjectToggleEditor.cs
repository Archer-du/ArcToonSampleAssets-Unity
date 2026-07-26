using UnityEditor;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Character.Editor
{
    [CustomEditor(typeof(ExclusiveObjectToggle))]
    public class ExclusiveObjectToggleEditor : UnityEditor.Editor
    {
        private SerializedProperty optionsProp;
        private SerializedProperty selectedIndexProp;

        private void OnEnable()
        {
            optionsProp = serializedObject.FindProperty("options");
            selectedIndexProp = serializedObject.FindProperty("selectedIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(optionsProp, true);

            // Popup entry 0 is "None" (selectedIndex = -1); entry i+1 is option i's name.
            string[] labels = BuildOptionLabels();
            int popup = Mathf.Clamp(selectedIndexProp.intValue + 1, 0, labels.Length - 1);
            popup = EditorGUILayout.Popup("Selected", popup, labels);
            selectedIndexProp.intValue = popup - 1;

            serializedObject.ApplyModifiedProperties();
        }

        private string[] BuildOptionLabels()
        {
            int count = optionsProp.arraySize;
            string[] labels = new string[count + 1];
            labels[0] = "None";
            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = optionsProp.GetArrayElementAtIndex(i);
                GameObject go = element.objectReferenceValue as GameObject;
                labels[i + 1] = go != null ? go.name : "(Missing)";
            }
            return labels;
        }
    }
}
