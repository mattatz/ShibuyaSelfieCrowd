using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(SaveBakedMesh))]
public class SaveBakedMeshEditor : Editor {

    protected string path = "Assets/BakedMesh.asset";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save"))
        {
            var baker = target as SaveBakedMesh;
            var mesh = baker.Bake();
            Save(mesh, path);
        }
    }

    protected void Save(Mesh mesh, string path)
    {
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

}
