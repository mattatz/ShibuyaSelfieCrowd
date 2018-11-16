using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VJ
{

    [CustomEditor (typeof(Inverse))]
    public class InverseEditor : Editor {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Toggle"))
            {
                var inverse = target as Inverse;
                inverse.Toggle();
            } else if(GUILayout.Button("Immediate"))
            {
                var inverse = target as Inverse;
                inverse.Immediate();
            }
        }

    }

}


