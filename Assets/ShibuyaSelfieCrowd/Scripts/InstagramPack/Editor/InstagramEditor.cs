using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace VJ
{

    [CustomEditor (typeof(Instagram))]
    public class InstagramEditor : Editor {

		public const string DIR_ASSETS = "Assets";
		public const string DIR_ROOT = "InstagramPack";

        protected string label = "Instagram";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            label = GUILayout.TextField(label);

            if(GUILayout.Button("Pack"))
            {
                var folderPath = AssureExistAndGetRootFolder();
                var tex = (target as Instagram).Pack();
                var path = folderPath + "/" + label + ".png";
                Save(tex, path);
            }
        }

        private static string AssureExistAndGetRootFolder() {
            var folderPath = DIR_ASSETS + "/" + DIR_ROOT;
            if (!Directory.Exists(folderPath)) {
                AssetDatabase.CreateFolder(DIR_ASSETS, DIR_ROOT);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return folderPath;
        }

        private static Texture2D Save (Texture2D tex, string pngPath) {
            #if UNITY_5_5_OR_NEWER
            File.WriteAllBytes (pngPath, tex.EncodeToPNG ());
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
            var pngImporter = (TextureImporter)AssetImporter.GetAtPath (pngPath);
            var pngSettings = new TextureImporterSettings ();
            pngImporter.ReadTextureSettings (pngSettings);
            pngSettings.mipmapEnabled = false;
            pngSettings.sRGBTexture = false;
            pngSettings.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SetTextureSettings (pngSettings);
            var platformSettings = pngImporter.GetDefaultPlatformTextureSettings ();
            platformSettings.format = TextureImporterFormat.RGB24;
            platformSettings.maxTextureSize = Mathf.Max (platformSettings.maxTextureSize, Mathf.Max (tex.width, tex.height));
            platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
            pngImporter.SetPlatformTextureSettings (platformSettings);
            AssetDatabase.WriteImportSettingsIfDirty (pngPath);
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);

            #else
            File.WriteAllBytes (pngPath, tex.EncodeToPNG ());
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
            var pngImporter = (TextureImporter)AssetImporter.GetAtPath (pngPath);
            var pngSettings = new TextureImporterSettings ();
            pngImporter.ReadTextureSettings (pngSettings);
            pngSettings.mipmapEnabled = false;
            pngSettings.linearTexture = true;
            pngSettings.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SetTextureSettings (pngSettings);
            pngImporter.textureFormat = TextureImporterFormat.RGB24;
            pngImporter.maxTextureSize = Mathf.Max (pngImporter.maxTextureSize, Mathf.Max (tex.width, tex.height));
            pngImporter.SaveAndReimport();
            #endif

			return (Texture2D)AssetDatabase.LoadAssetAtPath (pngPath, typeof(Texture2D));
		}

    }

}


