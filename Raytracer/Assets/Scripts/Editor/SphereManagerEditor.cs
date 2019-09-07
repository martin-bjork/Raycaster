using UnityEditor;
using UnityEngine;

namespace RaytracingEngine.Editors {

    [CustomEditor(typeof(SphereManager))]
    public class SphereManagerEditor : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Spawn spheres")) {
                ((SphereManager)target).SpawnSpheres();
            }
        }

    }

}
