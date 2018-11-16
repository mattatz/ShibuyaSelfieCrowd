using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VJ
{

    public abstract class CameraViewBase : ControllableBase {

        public virtual bool Fog { get { return fog; } }
        public virtual float FocusDistance { get { return 30f; } }
        public virtual float FocalLength { get { return 60f; } }

        [SerializeField] protected Camera cam;
        [SerializeField] protected float delta = 1f;
        [SerializeField] protected bool fog = false;
        protected bool activated;

        protected Vector3 position;
        protected Quaternion rotation = Quaternion.identity;

        public abstract ViewType GetViewType();

        protected virtual void OnEnable()
        {
            cam = Camera.main;
        }

        protected virtual void Start () {
        }
        
        protected virtual void Update () {
        }

        public virtual void Setup()
        {
            activated = true;
        }

        public virtual void Disable()
        {
            activated = false;
        }

        public virtual void Rotate(float t) { }

        public virtual void Zoom(float t) { }

        public virtual void Randomize() { }

        public virtual void Apply(float dt)
        {
            var dtt = dt * delta;
            cam.transform.position = Vector3.Lerp(cam.transform.position, position, dtt);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotation, dtt);
        }

    }

}


