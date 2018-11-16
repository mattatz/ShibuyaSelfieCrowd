using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class LightController : ControllableBase {

        [SerializeField] protected Color skyboxColor = Color.black;
        [SerializeField, Range(0.5f, 10f)] protected float skyboxIntensity = 1f;

        [SerializeField] protected Light environment;
        [SerializeField, Range(0f, 1f)] protected float envIntensity = 1f;
        protected float _envIntensity;

        [SerializeField] ReactiveLight center;

        [SerializeField] protected bool skyboxReactive, centerReactive;

        protected void OnEnable()
        {
            _envIntensity = envIntensity;
        }

        protected void Start () {
        }
        
        protected void Update () {
            var dt = Time.deltaTime;
            _envIntensity = Mathf.Lerp(_envIntensity, envIntensity, dt);

            environment.intensity = _envIntensity;
            skyboxColor = Color.Lerp(skyboxColor, Color.black, dt);

            RenderSettings.skybox.SetColor("_Color1", skyboxColor);
            RenderSettings.skybox.SetFloat("_Intensity", skyboxIntensity);

            if(Random.value < 0.1f)
            {
                center.Flash();
            }
        }

        public void OnPeak()
        {
            Peak();
        }

        protected void Peak()
        {
            if (skyboxReactive) skyboxColor = Color.white;
            if (centerReactive) center.Flash();
        }

        protected void OnDestroy()
        {
        }

        public override void Knob(int knobNumber, float knobValue)
        {
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
            switch(note)
            {
                case 36:
                    envIntensity = Mathf.Clamp01(1f - envIntensity);
                    break;

            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/skybox/reactive":
                    skyboxReactive = OSCUtils.GetBoolFlag(data);
                    break;
                case "/center/reactive":
                    centerReactive = OSCUtils.GetBoolFlag(data);
                    break;

                case "/skybox/color":
                    skyboxColor = OSCUtils.GetColorValue(data);
                    break;

                case "/skybox/intensity":
                    skyboxIntensity = OSCUtils.GetFValue(data);
                    break;

                case "/light/env":
                    envIntensity = OSCUtils.GetFValue(data);
                    break;

                case "/light/center":
                    break;

                case "/light/center/flash":
                    center.Flash();
                    break;
            }
        }

    }

}


