using UnityEngine;
using UnityEditor;

namespace Bukur
{
    [CustomEditor(typeof(PageGenerator))]
    [CanEditMultipleObjects]
    public class PageInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate"))
            {
                ((PageGenerator) target).Generate();
            }
            
            if (GUILayout.Button("Reinstantiate mesh"))
            {
                ((PageGenerator) target).InstantiateMesh();
            }
            
            if (GUILayout.Button("Create handle"))
            {
                ((PageGenerator) target).CreateHandle();
            }
            
            if (GUILayout.Button("Reinstantiate material"))
            {
                ((PageGenerator) target).InstantiateMaterial();
            }
            base.OnInspectorGUI();
        }
    }
}