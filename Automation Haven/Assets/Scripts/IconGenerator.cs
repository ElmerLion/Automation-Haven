using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEngine.GraphicsBuffer;

public class IconGenerator : MonoBehaviour {
    public RenderTexture renderTexture;


    public void SaveRenderTextureAfterRender() {
        // Wait for the end of the frame to ensure all rendering is complet

        SaveRenderTextureToPNG(renderTexture, Application.dataPath + "/SavedTextures/");
    }

    public void SaveRenderTextureToPNG(RenderTexture rt, string path) {
        // Activate the render texture for reading
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // Convert to PNG
        byte[] bytes = texture.EncodeToPNG();

        // Check if directory exists, if not, create it
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        // Construct a unique filename using the current date/time
        string filename = $"RenderTexture_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";

        // Write the PNG file to disk
        File.WriteAllBytes(Path.Combine(path, filename), bytes);

        Debug.Log($"Texture saved as PNG to {Path.Combine(path, filename)}");

        // Clean up
        texture = null;
        RenderTexture.active = null; // Reset the active RenderTexture
    }
}
