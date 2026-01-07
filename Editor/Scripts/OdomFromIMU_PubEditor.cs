using ROS.Publishers;
using UnityEditor;

namespace Editor.Scripts
{
    [CustomEditor(typeof(OdomFromIMU_Pub))]
    public class OdomFromIMU_PubEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();

            CreateReferenceFrameWarning();

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateReferenceFrameWarning()
        {
            SerializedProperty chosenFrame = serializedObject.FindProperty("targetSpace");
            if (chosenFrame.intValue != 0)
            {
                EditorGUILayout.HelpBox(
                    "Changing the default reference frame could cause inconsistencies!",
                    MessageType.Warning
                );
            }
        }
    }
}