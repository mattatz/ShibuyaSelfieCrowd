using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class AudioSignalController : ControllableBase {

        [SerializeField] protected AudioReactor highpass, bandpass, lowpass;
        [SerializeField, Range(1, 30)] protected int randomFrameMin = 3, randomFrameMax = 10;

        protected int highStep = 5, lowStep = 5;
        protected bool randomHigh, randomLow;

        
        void Update () {
            if(randomHigh)
            {
                if(Time.frameCount % highStep == 0)
                {
                    highpass.Peak();
                    highStep = Random.Range(randomFrameMin, randomFrameMax);
                }
            }

            if(randomLow)
            {
                if(Time.frameCount % lowStep == 0)
                {
                    lowpass.Peak();
                    lowStep = Random.Range(randomFrameMin, randomFrameMax);
                }
            }
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 20:
                    highpass.threshold = knobValue;
                    break;
                case 4:
                    // lowpass.threshold = knobValue;
                    bandpass.threshold = knobValue;
                    break;
            }
        }

        public override void NoteOn(int note)
        {
            switch(note)
            {
                case 52:
                    randomHigh = true;
                    break;
                case 68:
                    randomLow = true;
                    break;
            }
        }

        public override void NoteOff(int note)
        {
            switch(note)
            {
                case 52:
                    randomHigh = false;
                    break;
                case 68:
                    randomLow = false;
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/audio_reactor/lowpass/threshold":
                    lowpass.threshold = OSCUtils.GetFValue(data);
                    break;
                case "/audio_reactor/bandpass/threshold":
                    bandpass.threshold = OSCUtils.GetFValue(data);
                    break;
                case "/audio_reactor/highpass/threshold":
                    highpass.threshold = OSCUtils.GetFValue(data);
                    break;
            }
        }

    }

}


