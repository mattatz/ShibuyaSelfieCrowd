using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBakedMesh : MonoBehaviour {

    public SkinnedMeshRenderer Skin { get { return skin; } }

    [SerializeField] protected SkinnedMeshRenderer skin;

    public Mesh Bake()
    {
        var mesh = new Mesh();
        skin.BakeMesh(mesh);
        return mesh;
    }

}
