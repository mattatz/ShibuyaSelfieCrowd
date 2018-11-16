using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class FirstPersonView : HandyCameraViewBase
    {

        public override float FocusDistance { get { return focusDistance; } }
        public override float FocalLength { get { return focalLength; } }

        [SerializeField] protected Transform center;
        [SerializeField, Range(0f, 5f)] protected float distance = 3f, focusDistance;
        [SerializeField] protected float focalLength = 40;
        [SerializeField] protected Vector2 distanceRange = new Vector2(2f, 4f);

        [SerializeField] PolarCoordinate polar;
        [SerializeField] protected float speed = 1f;
        [SerializeField] protected Vector3 offset;

        [SerializeField] protected bool reactive;

        protected override void Update()
        {
            base.Update();

            var dt = Time.deltaTime;

            polar.theta1 += dt * speed;

            var p = center.position + offset + polar.Cartesian(distance);
            var dir = p - center.position;
            var r = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up);
            position = p + WobblePosition(dt);
            rotation = r * WobbleRotation(dt);
        }

        public override ViewType GetViewType()
        {
            return ViewType.FirstPerson;
        }

        public override void Rotate(float t) {
        }

        public override void Zoom(float t)
        {
            var d = Mathf.Lerp(distanceRange.y, distanceRange.x, t);
            distance = d;
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
        }

    }

}

