#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoesOfTheRuins.Editor
{
    public static class ProductionPreviewCapture
    {
        [MenuItem("Echoes/Capture Production Preview")]
        public static void CaptureFromCommandLine()
        {
            EditorSceneManager.OpenScene(ProductionSceneGenerator.ProductionScenePath);
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) throw new MissingReferenceException("Production scene has no camera.");

            const int width = 1600;
            const int height = 900;
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture cameraTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                image.Apply();
                Directory.CreateDirectory("Evidence");
                File.WriteAllBytes("Evidence/production-preview.png", image.EncodeToPNG());
                Debug.Log("Captured Evidence/production-preview.png");
            }
            finally
            {
                camera.targetTexture = cameraTarget;
                RenderTexture.active = previous;
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(image);
            }
        }
    }
}
#endif
