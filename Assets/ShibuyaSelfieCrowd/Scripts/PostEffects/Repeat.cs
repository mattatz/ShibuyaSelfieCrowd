using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class Repeat : PostEffectBase
    {

        [Range(1, 32)] public float count = 8;
        [Range(0f, 1f)] public float offset = 0f;
        public bool horizontal = true;
        public bool randomize = false;

        protected float randomSeed;

        protected void Start()
        {
        }

        protected void Update()
        {
            float u = 1f / count;
            material.SetFloat("_Count", count);
            material.SetFloat("_Unit", u);
            material.SetFloat("_Offset", Mathf.Lerp(0f, 1f - u, offset));
            material.SetFloat("_Horizontal", horizontal ? 1f : 0f);
            material.SetFloat("_Randomize", randomize ? 1f : 0f);
            material.SetFloat("_RandomSeed", randomSeed);

            // if(Random.value < 0.5f) Randomize();
        }

        public void Randomize()
        {
            randomSeed = Random.value;
        }

        public override void Knob(int knobNumber, float knobValue)
        {
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/posteffects/repeat/count":
                    count = Mathf.Max(1, OSCUtils.GetFValue(data));
                    break;

                case "/posteffects/repeat/horizontal":
                    horizontal = OSCUtils.GetBoolFlag(data);
                    break;

                case "/posteffects/repeat/offset":
                    offset = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/repeat/random":
                    randomize = OSCUtils.GetBoolFlag(data);
                    break;

                case "/posteffects/repeat/randomize":
                    Randomize();
                    break;
            }
        }

    }

}


