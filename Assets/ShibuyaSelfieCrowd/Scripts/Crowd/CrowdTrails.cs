using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ProcSkinAnim.Demo;

namespace VJ
{

    public class CrowdTrails : ProceduralSkinTrails
    {

        [SerializeField] protected float _throttle;
        [SerializeField, Range(0.1f, 30f)] protected float delta = 10f;
        [SerializeField] protected bool reactive = false;


        protected void OnEnable()
        {
            _throttle = throttle;
        }

        protected override void Update()
        {
            var dt = Time.deltaTime;
            throttle = Mathf.Lerp(throttle, _throttle, dt * delta);
            if(reactive)
            {
                _throttle = Mathf.Clamp01(_throttle - dt);
            }

            base.Update();
        }

        public void Peak()
        {
            if (!reactive) return;
            throttle = _throttle = Random.Range(0.8f, 1f);
        }

        public void OnKnob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 19:
                    _throttle = knobValue;
                    break;
                case 3:
                    attractorSpread = Mathf.Lerp(1f, 10f, knobValue);
                    break;
            }
        }

        public void OnNoteOn(int note)
        {
            switch(note)
            {
                case 35:
                    break;
                case 51:
                    break;
                case 66:
                    break;
            }
        }

        public void OnOSC(string address, List<object> data)
        {

            switch(address)
            {

                case "/trails/throttle":
                    _throttle = OSCUtils.GetFValue(data, 0);
                    break;

                case "/trails/drag":
                    drag = OSCUtils.GetFValue(data);
                    break;

                case "/trails/spread":
                    attractorSpread = OSCUtils.GetFValue(data);
                    break;

            }

        }

    }

}


