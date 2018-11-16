using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    [System.Serializable]
    public class AudioReaction
    {
        public float Peak { get { return peak; } set { peak = value; } }
        public bool On { get { return on; } }

        [SerializeField] protected float peak = 0.2f;
        [SerializeField] protected bool on;

        public AudioReaction(float peak, bool on = false)
        {
            this.peak = peak;
            this.on = on;
        }

        public bool Trigger(float v)
        {
            var ret = v > peak;
            if(ret != on) {
                on = ret;
                return true;
            }
            return false;
        }
    }

    public interface IAudioReactable {

        void OnReact(int index, bool on);

    }

}


