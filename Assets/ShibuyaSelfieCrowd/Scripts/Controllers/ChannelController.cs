using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class ChannelController : ControllableBase {

        [SerializeField] protected Material material;
        [SerializeField] protected Color color = Color.white;
        [SerializeField, Range(0f, 1f)] protected float alpha = 0f;
        protected float _alpha;

        [SerializeField] protected bool inverse = true;

        protected void OnEnable()
        {
            _alpha = alpha;
        }

        protected void Update () {
            var dt = Time.deltaTime;

            _alpha = Mathf.Lerp(_alpha, alpha, dt * 2f);

            material.SetColor("_Color", color);;
            material.SetFloat("_Alpha", _alpha);
            material.SetFloat("_Inverse", inverse ? 1f : 0f);
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
                case "/channel/alpha":
                    alpha = OSCUtils.GetFValue(data);
                    break;

                case "/channel/inverse":
                    inverse = OSCUtils.GetBoolFlag(data);
                    break;
            }
        }



    }

}


