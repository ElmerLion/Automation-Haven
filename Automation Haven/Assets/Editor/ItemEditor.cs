using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemEditor : Editor {
    private bool viewFromTop = false;
    private int rotationAngle = 0;
    private int tiltAngle = 0;
    private int prefabYRotation = 0;
    private Vector3 prefabPositionOffset = Vector3.zero;
    private float zoomLevel = 1.0f;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Icon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        ItemSO item = (ItemSO)target;

        viewFromTop = GUILayout.Toggle(viewFromTop, "View from Top");

        EditorGUILayout.LabelField("Camera Rotation Angle");
        rotationAngle = EditorGUILayout.IntSlider(rotationAngle, 0, 360);

        if (viewFromTop) {
            EditorGUILayout.LabelField("Prefab Tilt Angle");
            tiltAngle = EditorGUILayout.IntSlider(tiltAngle, 0, 360);
        }

        EditorGUILayout.LabelField("Prefab Y Rotation");
        prefabYRotation = EditorGUILayout.IntSlider(prefabYRotation, 0, 360);

        EditorGUILayout.LabelField("Prefab Position Offset");
        prefabPositionOffset = EditorGUILayout.Vector3Field("", prefabPositionOffset);

        EditorGUILayout.LabelField("Zoom Level");
        zoomLevel = EditorGUILayout.Slider(zoomLevel, 0.1f, 10.0f);

        if (item.sprite != null) {
            GUILayout.Label(item.sprite.texture, GUILayout.Width(64), GUILayout.Height(64));
        }

        if (GUILayout.Button("Generate Icon")) {
            if (item.prefab != null) {
                Texture2D texture = GeneratePreview(item.prefab.gameObject, 128, 128, viewFromTop, rotationAngle, tiltAngle, prefabYRotation, prefabPositionOffset);
                if (texture != null) {
                    string directory = "Assets/Textures/Generated";
                    if (!Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }
                    string texturePath = Path.Combine(directory, item.name + "_Icon.png");

                    File.WriteAllBytes(texturePath, texture.EncodeToPNG());
                    AssetDatabase.Refresh();

                    // Load and set import settings for the texture
                    TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (importer != null) {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spritePixelsPerUnit = 100;
                        importer.alphaIsTransparency = true;
                        importer.SaveAndReimport();
                    }

                    Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    item.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    EditorUtility.SetDirty(item); // Mark the item as dirty to save changes
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        ItemSO item = (ItemSO)target;

        if (item.sprite != null) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            EditorUtility.CopySerialized(item.sprite.texture, texture);
            return texture;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

        private int IntSliderWithTextField(int value, int leftValue, int rightValue) {
        EditorGUILayout.BeginHorizontal();
        value = EditorGUILayout.IntSlider(value, leftValue, rightValue);
        value = EditorGUILayout.IntField(value, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        return Mathf.Clamp(value, leftValue, rightValue);
    }

    private Texture2D GeneratePreview(GameObject prefab, int width, int height, bool viewFromTop, int rotationAngle, int tiltAngle, int prefabYRotation, Vector3 positionOffset) {
        // Create a temporary layer for preview
        int previewLayer = 31; // Make sure layer 31 is not used

        // Create a temporary camera
        GameObject tempCameraGO = new GameObject("TempCamera");
        Camera tempCamera = tempCameraGO.AddComponent<Camera>();
        tempCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.orthographic = true;
        tempCamera.cullingMask = 1 << previewLayer;

        // Instantiate the prefab temporarily
        GameObject tempPrefabInstance = Instantiate(prefab, positionOffset, Quaternion.identity);
        SetLayerRecursively(tempPrefabInstance, previewLayer);
        Bounds bounds = CalculateBounds(tempPrefabInstance);

        // Rotate the prefab and adjust its tilt
        tempPrefabInstance.transform.rotation = Quaternion.Euler(viewFromTop ? tiltAngle : 0, prefabYRotation, 0);

        // Adjust the camera orthographic size and position
        tempCamera.orthographicSize = bounds.extents.magnitude / zoomLevel;
        if (viewFromTop) {
            tempCamera.transform.position = bounds.center + Vector3.up * (bounds.extents.magnitude + 1) + Vector3.up * Mathf.Tan(Mathf.Deg2Rad * tiltAngle) * bounds.extents.magnitude;
            tempCamera.transform.rotation = Quaternion.Euler(90, rotationAngle, 0);
        } else {
            tempCamera.transform.position = bounds.center - Vector3.forward * (bounds.extents.magnitude + 1);
            tempCamera.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }

        // Create a render texture
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        tempCamera.targetTexture = renderTexture;

        // Render the prefab
        tempCamera.transform.position += positionOffset;
        tempCamera.Render();

        // Convert RenderTexture to Texture2D
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // Clean up
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        DestroyImmediate(tempCameraGO);
        DestroyImmediate(tempPrefabInstance);
        renderTexture.Release();

        return texture;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer) {
        if (obj == null) {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform) {
            if (child == null) {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private Bounds CalculateBounds(GameObject go) {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            return new Bounds(go.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private Sprite TextureToSprite(Texture2D texture) {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
