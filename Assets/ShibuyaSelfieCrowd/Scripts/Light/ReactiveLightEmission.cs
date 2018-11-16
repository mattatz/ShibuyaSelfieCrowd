using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class ReactiveLightEmission : ReactiveLight {

        [SerializeField] new protected Renderer renderer;
        [SerializeField, Range(0f, 1f)] protected float emissionIntensityMin = 0.1f;
        protected MaterialPropertyBlock block;

        protected override void OnEnable()
        {
            base.OnEnable();
            if(renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
            block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
        }

        protected override void Update()
        {
            base.Update();

            block.SetColor("_EmissionColor", color * Mathf.Max(emissionIntensityMin, intensity));
            renderer.SetPropertyBlock(block);
        }

    }

}


