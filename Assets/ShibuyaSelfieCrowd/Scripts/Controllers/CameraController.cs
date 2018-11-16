using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace VJ
{

    public enum ViewType
    {
        Free = 0,
        Bird = 1,
        Fixed = 2,
        Polar = 3,
        FirstPerson = 4,
    };

    public class CameraController : ControllableBase {

        public ViewType CurrentType { get { return type; } }

        [SerializeField] protected ViewType type = ViewType.Bird;
        [SerializeField] protected List<CameraViewBase> views;
        CameraViewBase current;

        [SerializeField] protected PostProcessVolume volume;
        [SerializeField] protected DepthOfField dof;
        [SerializeField] protected EdgeDetect_AfterStack edgeDetection;

        [SerializeField] protected Inverse inverse;
        [SerializeField] protected Distortion distortion;
        [SerializeField] protected Mirror mirror;
        [SerializeField] protected BlockNoise blockNoise;
        [SerializeField] protected Repeat repeat;

        protected void OnEnable()
        {
            volume.profile.TryGetSettings(out dof);
            volume.profile.TryGetSettings(out edgeDetection);
        }

        protected void Start () {
            views = GetComponentsInChildren<CameraViewBase>().ToList();
            UpdateView(type);
        }
        
        protected void Update () {
            var dt = Time.deltaTime;
            current.Apply(dt);

            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, current.Fog ? 0.0075f : 0f, dt);

            // https://github.com/Unity-Technologies/PostProcessing/blob/v2/PostProcessing/Runtime/ParameterOverride.cs
            dof.focusDistance.Interp(dof.focusDistance.value, current.FocusDistance, dt);
            dof.focalLength.Interp(dof.focalLength.value, current.FocalLength, dt);

            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                var current = Mathf.Max((int)type - 1, (int)ViewType.Bird);
                UpdateView((ViewType)current);
            } else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                var current = Mathf.Min((int)type + 1, (int)ViewType.FirstPerson);
                UpdateView((ViewType)current);
            } else if(Input.GetKeyDown(KeyCode.Space))
            {
                current.Randomize();
            }

        }

        public void UpdateView(ViewType t)
        {
            type = t;

            if (!Application.isPlaying) return;

            if(current != null)
            {
                current.Disable();
            }
            current = GetCurrentView(type);
            current.Setup();
        }

        protected CameraViewBase GetCurrentView(ViewType t)
        {
            var view = views.Find(v => v.GetViewType() == t);
            return view;
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 21:
                    current.Rotate(knobValue);
                    break;
                case 5:
                    current.Zoom(knobValue);
                    break;

                case 22:
                    edgeDetection.t.value = knobValue;
                    break;
                case 6:
                    repeat.count = Mathf.Lerp(1f, 32f, knobValue);
                    break;

                case 23:
                    distortion.t = knobValue;
                    break;
                case 7:
                    blockNoise.speed = Mathf.Lerp(0f, 20f, knobValue);
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
                case 37:
                    current.Randomize();
                    break;
                case 53:
                    break;
                case 69:
                    break;


                case 38:
                    repeat.randomize = !repeat.randomize;
                    break;
                case 54:
                    repeat.horizontal = !repeat.horizontal;
                    break;
                case 70:
                    break;

                case 39:
                    inverse.Toggle();
                    break;
                case 55:
                    mirror.vertical = !mirror.vertical;
                    break;
                case 71:
                    mirror.horizontal = !mirror.horizontal;
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/camera/view":
                    UpdateView((ViewType)OSCUtils.GetIValue(data, 0));
                    break;

                case "/posteffects/edge_detection/t":
                    edgeDetection.t.value = OSCUtils.GetFValue(data);
                    break;

                case "/posteffects/edge_detection/edge_color":
                    edgeDetection.edgeColor.value = OSCUtils.GetColorValue(data);
                    break;

                case "/posteffects/edge_detection/bg_color":
                    edgeDetection.bgColor.value = OSCUtils.GetColorValue(data);
                    break;
            }
        }

    }

}


