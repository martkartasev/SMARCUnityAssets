using ROS.Publishers.GroundTruth;
using UnityEditor;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GT_Odom_Pub))]
    public class GT_Odom_PubEditor : UnityEditor.Editor
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