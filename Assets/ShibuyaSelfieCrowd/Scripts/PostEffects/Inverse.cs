using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace VJ
{

    public class Inverse : PostEffectBase {

        [SerializeField, Range(0f, 1f)] protected float t = 0f;
        protected float _t;

        [SerializeField] protected float speed = 10f;
        [SerializeField, Range(0f, 1f)] protected float depth = 0f;

        #region Monobehaviour functions

        protected void Start()
        {
            _t = t;
        }

        protected void Update()
        {
            _t = Mathf.Lerp(_t, t, Time.deltaTime * speed);
            material.SetFloat("_T", _t);
            material.SetFloat("_UseDepth", depth);
        }

        #endregion

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/posteffects/inverse":
                    break;

                case "/posteffects/inverse/t":
                    t = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/inverse/toggle":
                    t = Mathf.Clamp01(1f - t);
                    break;
            }
        }

        public void Toggle()
        {
            t = 1f - Mathf.RoundToInt(t);
        }

        public void Immediate()
        {
            _t = t = 1f - Mathf.RoundToInt(t);
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


