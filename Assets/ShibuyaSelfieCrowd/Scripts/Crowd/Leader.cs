using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class Leader : ControllableBase
    {

        [SerializeField] new protected Renderer renderer;
        [SerializeField] new protected Light light;

        protected void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                light.enabled = renderer.enabled = !renderer.enabled;
            }
        }

        protected void OnEnable()
        {
#if UNITY_EDITOR
#else
            renderer.enabled = false;
            light.enabled = false;
#endif
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
                case "/leader/enabled":
                    light.enabled = renderer.enabled = OSCUtils.GetBoolFlag(data);
                    break;
            }
        }

    }

}


