using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VJ
{

    public class FixedView : HandyCameraViewBase {

        public override bool Fog { get { return points[current].Fog; } }
        public override float FocusDistance { get { return points[current].FocusDistance; } }

        [SerializeField] protected Transform pointsRoot;
        protected List<ViewPoint> points;
        [SerializeField] protected int current;

        public override ViewType GetViewType()
        {
            return ViewType.Fixed;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            points = pointsRoot.GetComponentsInChildren<ViewPoint>().ToList();
            current = 0;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            var dt = Time.deltaTime;
            position = points[current].transform.position + WobblePosition(dt);
            rotation = points[current].transform.rotation * WobbleRotation(dt);

            if(Input.GetKeyDown(KeyCode.R))
            {
                Randomize();
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Prev();
            } 
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                Next();
            }
        }

        public override void Zoom(float t)
        {
        }

        public override void Randomize()
        {
            var indices = Enumerable.Range(0, points.Count).ToList();
            indices.Remove(current);

            current = indices[Random.Range(0, indices.Count)];
            Immediate();
        }

        protected void Immediate()
        {
            position = points[current].transform.position;
            rotation = points[current].transform.rotation;
            cam.transform.position = position;
            cam.transform.rotation = rotation;
        }

        public void Next()
        {
            current++;
            current = current % points.Count;
        }

        public void Prev()
        {
            current--;
            if(current < 0)
            {
                current = points.Count - 1;
            }
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
                case "/camera/view/fixed/randomize":
                    if (!activated) return;
                    Randomize();
                    break;

                case "/camera/view/fixed/next":
                    Next();
                    break;

                case "/camera/view/fixed/prev":
                    Prev();
                    break;

                case "/camera/view/fixed/point":
                    current = OSCUtils.GetIValue(data);
                    break;

            }
        }

    }

}


