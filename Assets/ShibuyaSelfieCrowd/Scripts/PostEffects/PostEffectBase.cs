using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    [RequireComponent (typeof(Camera))]
    public abstract class PostEffectBase : ControllableBase {

        [SerializeField] protected Material material;

        protected virtual void OnRenderImage (RenderTexture src, RenderTexture dst)
        {
            Graphics.Blit(src, dst, material);
        }

    }

}


