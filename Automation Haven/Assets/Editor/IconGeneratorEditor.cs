using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(IconGenerator))]
public class IconGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector(); // Draws the default inspector

        IconGenerator iconGenerator = (IconGenerator)target;

        if (GUILayout.Button("Generate Icon")) {
            iconGenerator.SaveRenderTextureAfterRender();
        }
    }
}
