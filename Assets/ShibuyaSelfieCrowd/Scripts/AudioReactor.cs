using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Lasp;

namespace VJ
{


    public class AudioReactor : MonoBehaviour {

        public float RMS { get { return rms; } }

        [SerializeField] protected FilterType type;
        [SerializeField] protected KeyCode key;
        [Range(0f, 1f)] public float threshold = 0.75f;
        [SerializeField] protected float rms;
        [SerializeField] protected bool once;
        protected bool prev;

        [SerializeField] protected UnityEvent onPeak;

        protected float kSilence = -40; // -40 dBFS = silence

        protected void Update()
        {
            rms = Lasp.AudioInput.CalculateRMSDecibel(type);
            rms = Mathf.Clamp01(1f - rms / kSilence);

            var over = rms >= threshold;
            if(over)
            {
                if(!once)
                {
                    Peak();
                } else if(once && !prev)
                {
                    Peak();
                }
            }
            prev = over;

            if(Input.GetKeyDown(key))
            {
                Peak();
            }
        }

        public void Peak()
        {
            onPeak.Invoke();
        }

    }

}


