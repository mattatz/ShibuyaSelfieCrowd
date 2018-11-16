using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class Distortion : PostEffectBase {

        [Range(0f, 1f)] public float t = 0f;
        public float speed = 3f;

        protected float offset = 0f;
        protected float _t = 0f;

        protected Coroutine co;

        protected void Start()
        {
            _t = t;
        }

        protected void Update()
        {
            var dt = Time.deltaTime;

            _t = Mathf.Lerp(_t, t, Mathf.Clamp01(dt));
            material.SetFloat("_T", _t);

            offset += dt * speed;
            material.SetFloat("_Offset", offset);

            // if(Input.GetKeyDown(KeyCode.Space)) Trigger();
        }

        public void Trigger()
        {
            if(co != null)
            {
                StopCoroutine(co);
            }

            offset = Random.value * 100f;
            co = StartCoroutine(ITrigger(0.75f, 1.5f, 0.25f));
        }

        protected IEnumerator ITrigger(float toT, float toSpeed, float duration)
        {
            yield return 0;

            var time = 0f;

            var fromT = _t;
            var fromSpeed = speed;

            var hd = duration * 0.5f;

            while(time < hd)
            {
                time += Time.deltaTime;
                yield return 0;

                var w = time / hd;
                _t = t = Mathf.Lerp(fromT, toT, Easing.Exponential.Out(w));
                speed = Mathf.Lerp(fromSpeed, toSpeed, Easing.Exponential.Out(w));
            }

            _t = t = toT;
            speed = toSpeed;

            time = 0f;

            while(time < duration)
            {
                time += Time.deltaTime;
                yield return 0;

                var w = time / duration;
                var ww = Easing.Linear(w);
                _t = t = Mathf.Lerp(toT, 0f, ww);
                speed = Mathf.Lerp(toSpeed, 1f, ww);
            }

            _t = t = 0f;
            speed = 1f;
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
                case "/posteffects/distortion/t":
                    t = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/distortion/speed":
                    speed = OSCUtils.GetFValue(data);
                    break;
            }
        }
    }

}


