using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public interface INanoKontrollable {

        void NoteOn(int note);
        void NoteOff(int note);
        void Knob(int knobNumber, float knobValue);

    }

}


