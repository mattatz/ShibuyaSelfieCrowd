using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace VJ
{

    [System.Serializable]
    public class DepthEvent : UnityEvent<RenderTexture> {}


    [RequireComponent (typeof(Camera))]
    public class OrthographicCaptureDepth : OrthographicCaptureBase {

        [SerializeField] protected Shader replacementShader, maskShader;
        [SerializeField] protected ComputeShader signedDistanceFieldGen;

        [SerializeField] protected RenderTexture depth, mask, signedDistanceField;
        [SerializeField] protected float spread = 30f;

        [SerializeField] protected DepthEvent onCaptureDepth, onCaptureSDF;

        protected void OnDestroy()
        {
            depth.Release();
            mask.Release();
            signedDistanceField.Release();
        }

        public override void OnUpdateBounds(Bounds bounds)
        {
            base.OnUpdateBounds(bounds);

            depth = Create(resolution, resolution, 24, RenderTextureFormat.RFloat, TextureWrapMode.Repeat);

            var cam = GetComponent<Camera>();

            cam.targetTexture = depth;
            // cam.RenderWithShader(replacementShader, "RenderType");
            cam.SetReplacementShader(replacementShader, "RenderType");
            cam.Render();

            cam.enabled = false;

            Material mat;

            mat = new Material(maskShader);
            mask = new RenderTexture(depth.width, depth.height, 0);
            mask.Create();
            Graphics.Blit(depth, mask, mat);
            Destroy(mat);

            signedDistanceField = new RenderTexture(depth.width, depth.height, 0);
            signedDistanceField.enableRandomWrite = true;
            signedDistanceField.Create();
            var kernel = signedDistanceFieldGen.FindKernel("Generate");
            signedDistanceFieldGen.SetTexture(kernel, "_Input", mask);
            signedDistanceFieldGen.SetTexture(kernel, "_Field", signedDistanceField);
            signedDistanceFieldGen.SetFloat("_Spread", spread);
            GPUHelper.Dispatch2D(signedDistanceFieldGen, kernel, signedDistanceField.width, signedDistanceField.height);

            onCaptureDepth.Invoke(depth);
            onCaptureSDF.Invoke(signedDistanceField);
        }

    }

}


