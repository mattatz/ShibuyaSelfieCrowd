using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace VJ
{

    public class FreeView : CameraViewBase
    {

        public enum MouseButton
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }

        public enum MouseMove
        {
            X = 0,
            Y = 1,
            ScrollWheel = 2
        }

        static readonly string[] mouseKeywords = new string[]
        {
            "Mouse X",
            "Mouse Y",
            "Mouse ScrollWheel"
        };

        public MouseMove zoomTrigger = MouseMove.ScrollWheel;
        public bool invertZoomDirection = false;
        public float zoomSpeed = 6f;

        public MouseButton rotateTrigger = MouseButton.Right;
        public bool invertRotateDirection = false;
        public float rotateSpeed = 3f;

        public MouseButton dragTrigger = MouseButton.Left;
        public bool invertPanDirection = false;
        public float dragSpeed = 3f;

        public override ViewType GetViewType()
        {
            return ViewType.Free;
        }


        protected override void Start()
        {
            base.Start();
        }

        protected override void Update() {
            base.Update();
        }

        public override void Setup()
        {
            base.Setup();

            this.position = cam.transform.position;
            this.rotation = cam.transform.rotation;
        }

        public override void Apply(float dt)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Zoom();
            Rotate();
            Pan();
            base.Apply(dt);
        }

        void Zoom() {
            float moveAmount = Input.GetAxis(mouseKeywords[(int)this.zoomTrigger]);

            if (moveAmount != 0)
            {
                float direction = this.invertZoomDirection ? -1 : 1;
                this.position = cam.transform.forward;
                this.position *= this.zoomSpeed * moveAmount * direction;
                this.position += cam.transform.position;
            }
        }

        protected void Rotate()
        {
            float direction = this.invertRotateDirection ? -1 : 1;
            float mouseX = Input.GetAxis(mouseKeywords[(int)MouseMove.X]) * direction;
            float mouseY = Input.GetAxis(mouseKeywords[(int)MouseMove.Y]) * direction;

            if (Input.GetMouseButton((int)this.rotateTrigger))
            {
                this.rotation = Quaternion.Euler(0, mouseX * this.rotateSpeed, 0) * this.rotation;
                this.rotation = Quaternion.AngleAxis(mouseY * this.rotateSpeed, Vector3.Cross(cam.transform.forward, Vector3.up)) * this.rotation;
            }
        }

        protected void Pan()
        {
            float direction = this.invertPanDirection ? 1 : -1;
            float mouseX = Input.GetAxis(mouseKeywords[(int)MouseMove.X]) * direction;
            float mouseY = Input.GetAxis(mouseKeywords[(int)MouseMove.Y]) * direction;

            if (Input.GetMouseButton((int)this.dragTrigger))
            {
                this.position = cam.transform.position;
                this.position += cam.transform.right * mouseX * dragSpeed;
                this.position += cam.transform.up * mouseY * dragSpeed;
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
        }

    }

}


