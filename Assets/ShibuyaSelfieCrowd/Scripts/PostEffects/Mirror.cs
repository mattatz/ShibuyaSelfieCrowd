
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VJ
{

    public class Mirror : PostEffectBase {

        public bool horizontal, vertical;
        public bool left, up;

        protected void Start()
        {
            Apply();
        }

        protected void Apply()
        {
            material.SetFloat("_Horizontal", horizontal ? 1f : 0f);
            material.SetFloat("_Left", left ? 1f : 0f);
            material.SetFloat("_Vertical", vertical ? 1f : 0f);
            material.SetFloat("_Up", up ? 1f : 0f);
        }

        protected void Update()
        {
            Apply();
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/posteffects/mirror/horizontal":
                    horizontal = OSCUtils.GetBoolFlag(data, 0);
                    left = OSCUtils.GetBoolFlag(data, 1);
                    break;
                case "/posteffects/mirror/vertical":
                    vertical = OSCUtils.GetBoolFlag(data, 0);
                    up = OSCUtils.GetBoolFlag(data, 1);
                    break;
            }
        }

        public override void NoteOn(int note)
        {
        }

        public override void Knob(int knobNumber, float knobValue)
        {
        }

        public override void NoteOff(int note)
        {
        }

    }

}


