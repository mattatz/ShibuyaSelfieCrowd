using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class ReactiveLight : MonoBehaviour {

        [SerializeField] protected Color color;
        [SerializeField] protected float intensity = 0f, decay = 5f;

        [SerializeField] new protected Light light;

        protected int nextFrame = 10;

        protected virtual void OnEnable()
        {
            if(light == null)
            {
                light = GetComponent<Light>();
            }
        }
        
        protected virtual void Update () {

            if(Time.frameCount % nextFrame == 0)
            {
                // Flash();
                nextFrame = Random.Range(2, 10);
            }

            var dt = Time.deltaTime;
            intensity = Mathf.Lerp(intensity, 0f, dt * decay);

            light.color = color;
            light.intensity = intensity;
        }

        public void Flash()
        {
            intensity = Random.Range(0.5f, 5f);
        }
      
    }

}


