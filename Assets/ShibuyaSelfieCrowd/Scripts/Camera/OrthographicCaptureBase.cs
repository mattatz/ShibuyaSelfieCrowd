using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    [RequireComponent(typeof(Camera))]
    public abstract class OrthographicCaptureBase : MonoBehaviour {

        [SerializeField, Range(128, 4096)] protected int resolution = 1024;

        public virtual void OnUpdateBounds(Bounds bounds)
        {
            var cam = GetComponent<Camera>();
            cam.aspect = bounds.size.x / bounds.size.z;
            cam.orthographicSize = bounds.size.x * 0.5f;
            cam.farClipPlane = bounds.size.y;
            transform.position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        protected RenderTexture Create(int w, int h, int depth, RenderTextureFormat format, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            var tex = new RenderTexture(w, h, depth);
            tex.format = format;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = wrapMode;
            tex.Create();
            return tex;
        }

    }

}


