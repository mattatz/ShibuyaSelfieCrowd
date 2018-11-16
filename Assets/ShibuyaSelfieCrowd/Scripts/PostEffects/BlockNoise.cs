using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VJ {

    public class BlockNoise : PostEffectBase
    {
        [Range(0f, 1f)] public float t = 0f;
        [Range(0f, 20f)] public float speed = 3f;
        [Range(0f, 0.25f)] public float shift = 0.025f;
        public Vector2 scaleS = new Vector2(24f, 9f), scaleL = new Vector2(8f, 4f);

        protected void Update()
        {
            material.SetFloat("_T", t);
            material.SetFloat("_Speed", speed);
            material.SetFloat("_Shift", shift);
            material.SetVector("_ScaleS", scaleS);
            material.SetVector("_ScaleL", scaleL);
        }

        public override void OnOSC(string addr, List<object> data)
        {
            switch(addr)
            {
                case "/posteffects/block_noise/t":
                    t = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/block_noise/shift":
                    shift = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/block_noise/speed":
                    speed = OSCUtils.GetFValue(data);
                    break;
            }
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
    }

}