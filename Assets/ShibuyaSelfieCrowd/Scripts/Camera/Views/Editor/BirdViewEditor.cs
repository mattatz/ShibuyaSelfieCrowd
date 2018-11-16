using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VJ
{

    [CustomEditor (typeof(BirdView))]
    public class BirdViewEditor : Editor {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var view = target as BirdView;
            if(GUILayout.Button("Randomize"))
            {
                view.Randomize();
            }
        }

    }

}


