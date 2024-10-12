using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CompanySO))]
public class CompanyEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        CompanySO companySO = (CompanySO)target;

        if (companySO.companyLogo != null) {
            GUILayout.Label(companySO.companyLogo.texture, GUILayout.Width(64), GUILayout.Height(64));
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        CompanySO companySO = (CompanySO)target;

        if (companySO.companyLogo != null) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            EditorUtility.CopySerialized(companySO.companyLogo.texture, texture);
            return texture;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

}
