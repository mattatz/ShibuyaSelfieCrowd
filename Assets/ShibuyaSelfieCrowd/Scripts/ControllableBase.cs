using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public abstract class ControllableBase : MonoBehaviour, INanoKontrollable, IOSCReactable
    {

        public abstract void Knob(int knobNumber, float knobValue);
        public abstract void NoteOff(int note);
        public abstract void NoteOn(int note);

        public abstract void OnOSC(string address, List<object> data);

    }

}


