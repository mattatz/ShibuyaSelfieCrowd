using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace VJ
{

    [System.Serializable]
    public class CityBoundsEvent : UnityEvent<Bounds> { }

    public class City : ControllableBase {

        [SerializeField] protected List<MeshRenderer> renderers;
        [SerializeField] protected CityBoundsEvent onUpdateBounds;
        [SerializeField] protected List<CityUnit> units;

        [SerializeField, ColorUsage(false, true)] protected Color emission;
        [SerializeField, Range(0f, 2.25f)] protected float emissionIntensity;

        [SerializeField, Range(0f, 1f)] protected float useTexture = 0f, center = 0f;
        protected float _center;

        // [SerializeField] protected bool wireframe = true;
        [SerializeField, Range(0f, 1f)] protected float wireframe = 0f, gain = 1f;
        [SerializeField, Range(0.5f, 30f)] protected float thickness = 5f;

        [SerializeField] protected bool reactive = false;

        [SerializeField, Range(0.001f, 0.05f)] public float noiseScale = 0.01f;
        [SerializeField, Range(0f, 10f)] public float noiseIntensity = 5f, noiseSpeed = 1f;
        protected float _noiseIntensity, _noiseSpeed;
        protected float noiseOffset;

        protected Coroutine iTrigger;
        protected Action callback;

        protected void OnEnable()
        {
            SetupBounds();

            _center = center;
            _noiseIntensity = noiseIntensity;
            _noiseSpeed = noiseSpeed;
        }

        protected void Awake()
        {
            SetupBounds();

            units = GetComponentsInChildren<CityUnit>().ToList();
        }

        protected void Update()
        {
            var dt = Time.deltaTime;

            _center = Mathf.Lerp(_center, center, dt * 10f);
            _noiseIntensity = Mathf.Lerp(_noiseIntensity, noiseIntensity, dt * 10f);
            _noiseSpeed = Mathf.Lerp(_noiseSpeed, noiseSpeed, dt * 10f);
            noiseOffset += _noiseSpeed * dt;

            units.ForEach(u =>
            {
                u.Emission = emission * emissionIntensity;

                u.useTexture = useTexture;
                u.center = _center;
                u.wireframe = wireframe;
                u.gain = gain * thickness;

                u.noiseScale = noiseScale;
                u.noiseIntensity = _noiseIntensity;
                u.noiseOffset = noiseOffset;
            });
        }

        protected void SetupBounds()
        {
            renderers = GetComponentsInChildren<MeshRenderer>().ToList();
            var bounds = renderers[0].bounds;
            renderers.ForEach(rnd =>
            {
                bounds.Encapsulate(rnd.bounds);
            });
            onUpdateBounds.Invoke(bounds);
        }

        public void Peak()
        {
            if (!reactive) return;
            // _noiseSpeed = Random.Range(8f, 10f);
            Trigger();
        }

        public void Randomize()
        {
            noiseIntensity = Mathf.Lerp(0f, 10f, Random.value);
            noiseScale = Mathf.Lerp(0.01f, 0.02f, Random.value);
            noiseOffset = Random.value * 100f;
            noiseSpeed = Mathf.Lerp(0f, 1f, Random.value);
        }

        public void RandomOffset()
        {
            noiseOffset += Random.value * 100f;
        }

        protected void Trigger()
        {
            StopTrigger();

            var tmpCenter = center;
            var tmpNoiseIntensity = noiseIntensity;
            callback = () => {
                center = tmpCenter;
                noiseIntensity = tmpNoiseIntensity;
            };

            var v = Random.Range(0.25f, 0.5f);
            _center = center = v;
            _noiseIntensity = noiseIntensity = Mathf.Lerp(0f, 18f, v);

            iTrigger = StartCoroutine(ITrigger(0.2f));
        }

        protected void StopTrigger()
        {
            if(iTrigger != null)
            {
                callback();
                StopCoroutine(iTrigger);
            }
            iTrigger = null;
        }

        protected IEnumerator ITrigger(float duration)
        {
            yield return new WaitForSeconds(duration);
            StopTrigger();
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 16:
                    center = Mathf.Clamp01(1f - knobValue);
                    break;
                case 0:
                    noiseIntensity = Mathf.Lerp(0f, 10f, knobValue);
                    break;
            }
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
            switch(note)
            {
                case 32:
                    wireframe = Mathf.Clamp01(1f - wireframe);
                    break;

                case 48:
                    useTexture = Mathf.Clamp01(1f - useTexture);
                    break;

                case 64:
                    Trigger();
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/city/center":
                    center = OSCUtils.GetFValue(data);
                    break;

                case "/city/wireframe":
                    wireframe = OSCUtils.GetFValue(data);
                    break;

                case "/city/emission/color":
                    emission = OSCUtils.GetColorValue(data);
                    break;

                case "/city/emission/intensity":
                    emissionIntensity = OSCUtils.GetFValue(data);
                    break;

                case "/city/texture":
                    useTexture = OSCUtils.GetFValue(data);
                    break;

                case "/city/noise/scale":
                    noiseScale = OSCUtils.GetFValue(data);
                    break;

                case "/city/noise/intensity":
                    noiseIntensity = OSCUtils.GetFValue(data);
                    break;

                case "/city/noise/speed":
                    noiseSpeed = OSCUtils.GetFValue(data);
                    break;

                case "/city/noise/reactive":
                    reactive = OSCUtils.GetBoolFlag(data);
                    break;

            }
        }

    }

}


