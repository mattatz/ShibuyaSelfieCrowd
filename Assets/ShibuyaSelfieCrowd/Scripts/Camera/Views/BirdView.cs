using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class BirdView : CameraViewBase {

        public override float FocusDistance { get { return distance; } }

        [SerializeField] protected float distance = 20f;
        protected float width, height;
        protected float hwidth, hheight;

        [SerializeField] protected Vector2 velocity;
        [SerializeField] protected Vector2 distanceRange = new Vector2(110f, 200f), speedRange = new Vector2(1f, 5f);

        protected Bounds WholeBounds { get { return CityGlobal.Instance.WholeBounds; } }

        protected float CornerLeft
        {
            get
            {
                return WholeBounds.min.x + hwidth;
            }
        }

        protected float CornerRight
        {
            get
            {
                return WholeBounds.max.x - hwidth;
            }
        }

        protected float CornerTop
        {
            get
            {
                return WholeBounds.min.z + hheight;
            }
        }

        protected float CornerBottom
        {
            get
            {
                return WholeBounds.max.z - hheight;
            }
        }

        public override ViewType GetViewType()
        {
            return ViewType.Bird;
        }

        protected override void Start()
        {
            base.Start();
            rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

            distance = Random.Range(distanceRange.x, distanceRange.y);
            UpdateDistance(distance);
            Randomize();
        }

        protected override void Update()
        {
            var dt = Time.deltaTime;
            position.x += velocity.x * dt;
            position.z += velocity.y * dt;

            if (position.x < CornerLeft || position.x > this.CornerRight)
            {
                velocity *= -1;
            }
            if (position.z < CornerTop || position.z > this.CornerBottom)
            {
                velocity.y *= -1;
            }

            // constrain
            position.x = Mathf.Clamp(position.x, CornerLeft, CornerRight);
            position.z = Mathf.Clamp(position.z, CornerTop, CornerBottom);

            // if(Input.GetKeyDown(KeyCode.R)) Randomize();

            base.Update();
        }

        public override void Zoom(float t)
        {
            var d = Mathf.Lerp(distanceRange.y, distanceRange.x, t);
            UpdateDistance(d);
        }

        public override void Randomize()
        {
            var d = (distance - distanceRange.x) / (distanceRange.y - distanceRange.x);
            // var distance = d * (distanceRange.y - distanceRange.x) + distanceRange.x;
            // UpdateDistance(distance);

            position.x = Random.Range(CornerLeft, CornerRight);
            position.z = Random.Range(CornerTop, CornerBottom);

            // var cur = cam.transform.position;
            // velocity.x = d * Mathf.Sign(position.x - cur.x) * Random.Range(speedRange.x, speedRange.y);
            // velocity.y = d * Mathf.Sign(position.z - cur.z) * Random.Range(speedRange.x, speedRange.y);

            cam.transform.position = position;

            var td = Mathf.Lerp(1f, 0.2f, d);
            velocity.x = td * Random.Range(speedRange.x, speedRange.y) * Mathf.Lerp(-1f, 1f, Random.value > 0.5f ? 1f : 0f);
            velocity.y = td * Random.Range(speedRange.x, speedRange.y) * Mathf.Lerp(-1f, 1f, Random.value > 0.5f ? 1f : 0f);
        }

        protected void UpdateDistance(float d)
        {
            position.y = distance = d;

            var vfov = cam.fieldOfView * Mathf.PI / 180f;
            height = 2f * Mathf.Tan(vfov / 2f) * distance;
            width = height * cam.aspect;

            hwidth = width * 0.5f;
            hheight = height * 0.5f;
        }

        protected void OnValidate()
        {
            if(Application.isPlaying && Application.isEditor)
            {
                UpdateDistance(distance);
            }
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            var lt = new Vector3(CornerLeft, distance, CornerTop);
            var rt = new Vector3(CornerRight, distance, CornerTop);
            var rb = new Vector3(CornerRight, distance, CornerBottom);
            var lb = new Vector3(CornerLeft, distance, CornerBottom);
            Gizmos.DrawLine(lt, rt);
            Gizmos.DrawLine(rt, rb);
            Gizmos.DrawLine(rb, lb);
            Gizmos.DrawLine(lb, lt);

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(WholeBounds.center, WholeBounds.size);
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
                case "/camera/view/bird/randomize":
                    if (!activated) return;
                    Randomize();
                    break;

                case "/camera/view/bird/distance":
                    distanceRange = OSCUtils.GetV2Value(data);
                    distance = Mathf.Clamp(distance, distanceRange.x, distanceRange.y);
                    UpdateDistance(distance);
                    break;
            }
        }

    }

}


