using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecipeSO))]
public class RecipeSOEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        RecipeSO recipe = (RecipeSO)target;

        if (recipe.output != null && recipe.output.Count > 0 && recipe.output[0].itemSO != null) {
            Sprite icon = recipe.output[0].itemSO.sprite;
            if (icon != null) {
                GUILayout.Label(icon.texture, GUILayout.Width(64), GUILayout.Height(64));
            }
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        RecipeSO recipe = (RecipeSO)target;

        if (recipe.output != null && recipe.output.Count > 0 && recipe.output[0].itemSO != null) {
            Sprite icon = recipe.output[0].itemSO.sprite;
            if (icon != null) {
                Texture2D texture = new Texture2D(width, height);
                EditorUtility.CopySerialized(icon.texture, texture);
                return texture;
            }
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
