using ROS.Publishers;
using UnityEditor;

namespace Editor.Scripts
{
    [CustomEditor(typeof(ROSTransformTreePublisher))]
    public class ROSTransformTreePublisherEditor : UnityEditor.Editor
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
            SerializedProperty chosenFrame = serializedObject.FindProperty("referenceFrame");
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