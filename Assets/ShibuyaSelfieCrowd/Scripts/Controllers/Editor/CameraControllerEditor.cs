using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VJ
{

    [CustomEditor (typeof(CameraController))]
    public class CameraControllerEditor : Editor {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var controller = target as CameraController;

            var names = Enum.GetNames(typeof(ViewType));
            var selected = (int)controller.CurrentType;
            int ret = GUILayout.SelectionGrid(selected, names, 2);
            if(ret != selected)
            {
                controller.UpdateView((ViewType)ret);
            }
        }

    }

}


