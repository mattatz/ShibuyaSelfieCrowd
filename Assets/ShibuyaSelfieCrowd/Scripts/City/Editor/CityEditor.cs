using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VJ
{

    [CustomEditor(typeof(City))]
    public class CityEditor : Editor {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Randomize"))
            {
                var city = target as City;
                city.Randomize();
            }
        }


    }

}


