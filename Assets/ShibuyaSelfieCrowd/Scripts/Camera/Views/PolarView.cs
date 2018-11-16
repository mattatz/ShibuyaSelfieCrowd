using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class PolarView : HandyCameraViewBase
    {
        public override float FocusDistance { get { return distance;} }

        [SerializeField] protected Transform center;
        [SerializeField, Range(5f, 120f)] protected float distance = 10f;
        [SerializeField] protected Vector2 distanceRange = new Vector2(5f, 100f);

        [SerializeField] PolarCoordinate polar;
        [SerializeField] protected float theta0Base = 0f;
        protected const float theta0Max = 1.45f;

        [SerializeField] protected bool reactive;

        public override ViewType GetViewType()
        {
            return ViewType.Polar;
        }

        protected override void Update()
        {
            base.Update();

            var dt = Time.deltaTime;

            var p = center.position + polar.Cartesian(distance);
            var r = Quaternion.LookRotation(center.position - p, Vector3.up);
            position = p + WobblePosition(dt);
            rotation = r * WobbleRotation(dt);
        }

        public override void Rotate(float t) {
            theta0Base = Mathf.Lerp(0f, theta0Max, t);
            polar.theta0 = theta0Base;
        }

        public override void Zoom(float t)
        {
            var d = Mathf.Lerp(distanceRange.y, distanceRange.x, t);
            distance = d;
        }

        public override void Randomize()
        {
            distance += Random.Range(-2f, 2f);
            distance = Mathf.Clamp(distance, distanceRange.x, distanceRange.y);

            polar.theta0 = Mathf.Min(theta0Base + Random.Range(0f, 0.2f), theta0Max);
            polar.theta1 = Random.Range(0.75f, 1.25f);
        }

        public void Peak()
        {
            if (!reactive) return;
            Randomize();
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
                case "/camera/view/polar/randomize":
                    if (!activated) return;
                    Randomize();
                    break;

                case "/camera/view/polar/reactive":
                    reactive = OSCUtils.GetBoolFlag(data);
                    break;
            }
        }

    }

}


