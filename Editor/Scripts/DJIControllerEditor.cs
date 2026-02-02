using UnityEditor;
using UnityEngine;

using dji;

namespace Editor.Scripts
{
    [CustomEditor(typeof(SimplerDJIController))]
    public class DJIControllerEditor : UnityEditor.Editor
    {
        SimplerDJIController container;

        public override void OnInspectorGUI()
        {
            container = (SimplerDJIController)target;
            DrawDefaultInspector();

            if (GUILayout.Button("TakeOff"))
            {
                container.TakeOff();
            }
            
            if (GUILayout.Button("Land"))
            {
                container.Land();
            }
        }
    }
}