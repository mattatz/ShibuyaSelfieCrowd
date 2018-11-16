using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace VJ
{

    [System.Serializable]
    public class ColorEvent : UnityEvent<RenderTexture> {}


    [RequireComponent (typeof(Camera))]
    public class OrthographicCaptureColor : OrthographicCaptureBase {

        [SerializeField] protected RenderTexture color;
        [SerializeField] protected ColorEvent onCapture;

        protected void OnDestroy()
        {
            color.Release();
        }

        public override void OnUpdateBounds(Bounds bounds)
        {
            base.OnUpdateBounds(bounds);
            color = Create(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, TextureWrapMode.Repeat);

            var cam = GetComponent<Camera>();

            cam.targetTexture = color;
            cam.Render();

            cam.enabled = false;

           onCapture.Invoke(color);
        }

    }

}


