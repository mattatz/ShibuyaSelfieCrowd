using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

using VertexAnimater;

namespace VJ
{

    public class Crowd : ControllableBase {

        protected enum CrowdMode
        {
            Stop = 0,
            Wander = 1,
            Gather = 2,
            Flow = 3,
            Boids = 4,
        };

        [SerializeField] protected Mesh mesh;
        [SerializeField] protected int count = 4096;
        [SerializeField, Range(0f, 1f)] protected float throttle = 1f;
        protected float _throttle;

        [SerializeField] protected float size = 10f;
        [SerializeField] protected Vector2 scaleRange = new Vector2(0.9f, 1.1f);
        [SerializeField] protected List<Box> boxes;

        [SerializeField] protected CrowdMode mode = CrowdMode.Boids;
        [SerializeField] protected Material render;
        [SerializeField] protected ComputeShader compute;

        [SerializeField, Range(0.5f, 50f)] protected float gradient = 15f;
        [SerializeField, Range(0.8f, 0.999f)] protected float decay = 0.975f;

        #region Wander params

        [SerializeField, Range(0f, 1.0f)] protected float wanderRadius = 0.5f, wanderDistance = 1f, wanderWeight = 0.1f;

        #endregion

        #region Gather params

        [SerializeField] protected Transform center;
        [SerializeField, Range(0f, 0.25f)] protected float gatherWeight = 0.1f;
        [SerializeField, Range(1f, 10f)] protected float gatherRadius = 3f;
        [SerializeField, Range(1f, 60f)] protected float gatherHole = 5f, gatherDistance = 45f;

        #endregion

        #region Flow params

        [SerializeField, Range(0f, 0.25f)] protected float flowWeight = 0.1f;
        [SerializeField] protected float flowRadius = 100f;

        #endregion

        #region Boids params

        [SerializeField] protected float separateNeighborhoodRadius = 20f, alignmentNeighborhoodRadius = 100f, cohesionNeighborhoodRadius = 200f;
        [SerializeField, Range(0f, 0.25f)] protected float separateWeight = 0.1f, alignmentWeight = 0.1f, cohesionWeight = 0.1f;
        [SerializeField] protected float maxSpeed = 1f, maxSteerForce = 1f;

        #endregion

        #region Face params

        [SerializeField, Range(0f, 1f)] protected float useFace = 0f;
        protected float _useFace;

        [SerializeField] protected float faceScrollSpeed = 1f;
        [SerializeField] protected float faceRandom;
        protected float faceOffset = 0f;

        #endregion

        #region Emission params

        [SerializeField] protected bool useEmission = true;
        [SerializeField, Range(0f, 0.5f)] protected float emissionRate = 0.01f;
        [SerializeField, Range(0f, 1.5f)] protected float emissionRandom = 0.01f;
        [SerializeField, Range(0f, 15.0f)] protected float emissionSpeed = 1.0f;
        protected float emissionOffset = 0f;

        #endregion

        protected PingPongBuffer crowdBuffer;
        protected ComputeBuffer argsBuffer;
        protected ComputeBuffer attributeBuffer, emitBoundsBuffer;

        [SerializeField] protected Material idle, walk;

        protected Dictionary<KeyCode, CrowdMode> keys;

        protected void OnEnable()
        {
            _throttle = throttle;
            _useFace = useFace;

            keys = new Dictionary<KeyCode, CrowdMode>()
            {
                { KeyCode.S, CrowdMode.Stop },
                { KeyCode.W, CrowdMode.Wander },
                { KeyCode.G, CrowdMode.Gather },
                { KeyCode.F, CrowdMode.Flow },
                { KeyCode.B, CrowdMode.Boids }
            };
        }

        protected void Start () {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

            crowdBuffer = new PingPongBuffer(count, typeof(Human));

            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)count;
            argsBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(uint)) * args.Length, ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);

            SetupVertexAnim();

            boxes = GetComponentsInChildren<Box>().ToList();

            emitBoundsBuffer = new ComputeBuffer(boxes.Count, Marshal.SizeOf(typeof(Matrix4x4)));
            var data = new Matrix4x4[boxes.Count];
            for(int i = 0, n = boxes.Count; i < n; i++)
            {
                var b = boxes[i];
                data[i] = b.transform.localToWorldMatrix;
            }
            emitBoundsBuffer.SetData(data);

            Init();
        }

        protected void Update () {
            var dt = Time.deltaTime;
            var t = Time.timeSinceLevelLoad;

            _throttle = Mathf.Lerp(_throttle, throttle, dt);
            _useFace = Mathf.Lerp(_useFace, useFace, dt);

            if (_throttle > 0f)
            {
                switch(mode)
                {
                    case CrowdMode.Wander:
                        Wander(t, dt);
                        break;
                    case CrowdMode.Gather:
                        Gather(t, dt);
                        break;
                    case CrowdMode.Flow:
                        Flow(t, dt);
                        break;
                    case CrowdMode.Boids:
                        Boids(t, dt);
                        break;
                    default: // Stop
                        break;
                }
            }

            foreach(var op in keys) if(Input.GetKeyDown(op.Key)) mode = op.Value;

            Update(t, dt);

            emissionOffset += dt * emissionSpeed;
            faceOffset += dt * faceScrollSpeed;
            Render();
        }
        
        protected void SetupGradient(int kernel)
        {
            var city = CityGlobal.Instance;
            compute.SetTexture(kernel, "_Depth", city.OrthographicDepth);
            compute.SetTexture(kernel, "_SDF", city.OrthographicSDF);
            compute.SetVector("_BoundsMin", city.WholeBounds.min);
            compute.SetVector("_BoundsMax", city.WholeBounds.max);
            compute.SetFloat("_Gradient", gradient);
        }

        protected void SetupGather()
        {
            compute.SetVector("_Center", center.position);
            compute.SetFloat("_GatherWeight", gatherWeight);
            compute.SetFloat("_GatherRadius", gatherRadius);
            compute.SetFloat("_GatherHole", gatherHole);
            compute.SetFloat("_GatherDistance", gatherDistance);
        }

        protected void Reset()
        {
            Init();
            mode = CrowdMode.Stop;
        }

        protected void Init()
        {
            var kernel = compute.FindKernel("Init");
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Read);
            compute.SetBuffer(kernel, "_EmitBounds", emitBoundsBuffer);
            compute.SetFloat("_Size", size);
            compute.SetVector("_ScaleRange", scaleRange);
            SetupGradient(kernel);
            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Update (float t, float dt) {
            var kernel = compute.FindKernel("Update");
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Read);
            compute.SetFloat("_Throttle", Mathf.Lerp(-0.01f, 1f, _throttle));
            compute.SetFloat("_Time", t);
            compute.SetFloat("_DT", dt);
            compute.SetFloat("_Decay", decay);
            compute.SetVector("_Center", center.position);
            SetupGradient(kernel);
            SetupGather();
            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Wander(float t, float dt)
        {
            var kernel = compute.FindKernel("Wander");
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Read);
            compute.SetFloat("_Time", t);
            compute.SetFloat("_DT", dt);

            SetupGradient(kernel);

            compute.SetFloat("_WanderRadius", wanderRadius);
            compute.SetFloat("_WanderDistance", wanderDistance);
            compute.SetFloat("_WanderWeight", wanderWeight);

            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Gather(float t, float dt)
        {
            var kernel = compute.FindKernel("Gather");
            compute.SetBuffer(kernel, "_CrowdRead", crowdBuffer.Read);
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Write);
            compute.SetFloat("_Time", t);
            compute.SetFloat("_DT", dt);

            SetupGradient(kernel);
            SetupGather();

            compute.SetFloat("_SeparateNeighborhoodRadius", separateNeighborhoodRadius);
            compute.SetFloat("_SeparateWeight", separateWeight);
            compute.SetFloat("_MaxSpeed", maxSpeed);
            compute.SetFloat("_MaxSteerForce", maxSteerForce);

            GPUHelper.Dispatch1D(compute, kernel, count);

            crowdBuffer.Swap();
        }

        protected void Flow(float t, float dt)
        {
            var kernel = compute.FindKernel("Flow");
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Read);
            compute.SetFloat("_Time", t);
            compute.SetFloat("_DT", dt);

            SetupGradient(kernel);
            SetupGather();

            compute.SetVector("_Center", center.position);
            compute.SetFloat("_FlowWeight", flowWeight);
            compute.SetFloat("_FlowRadius", flowRadius);
            compute.SetFloat("_MaxSpeed", maxSpeed);
            compute.SetFloat("_MaxSteerForce", maxSteerForce);

            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Boids(float t, float dt)
        {
            var kernel = compute.FindKernel("Boids");
            compute.SetBuffer(kernel, "_CrowdRead", crowdBuffer.Read);
            compute.SetBuffer(kernel, "_Crowd", crowdBuffer.Write);
            compute.SetFloat("_Time", t);
            compute.SetFloat("_DT", dt);

            SetupGradient(kernel);

            compute.SetFloat("_SeparateNeighborhoodRadius", separateNeighborhoodRadius);
            compute.SetFloat("_AlignmentNeighborhoodRadius", alignmentNeighborhoodRadius);
            compute.SetFloat("_CohesionNeighborhoodRadius", cohesionNeighborhoodRadius);
            compute.SetFloat("_SeparateWeight", separateWeight);
            compute.SetFloat("_AlignmentWeight", alignmentWeight);
            compute.SetFloat("_CohesionWeight", cohesionWeight);
            compute.SetFloat("_MaxSpeed", maxSpeed);
            compute.SetFloat("_MaxSteerForce", maxSteerForce);

            GPUHelper.Dispatch1D(compute, kernel, count);

            crowdBuffer.Swap();
        }

        protected void Render () {
            render.SetBuffer("_Crowd", crowdBuffer.Read);
            render.SetBuffer("_Attributes", attributeBuffer);

            render.SetFloat("_UseFace", _useFace);
            render.SetFloat("_FaceRandom", faceRandom);
            render.SetVector("_Scroll", new Vector2(faceOffset, -faceOffset));

            render.SetFloat("_EmissionRate", emissionRate * (useEmission ? 1f : 0f));
            render.SetFloat("_EmissionOffset", emissionOffset);
            render.SetFloat("_EmissionRandom", emissionRandom);

            Graphics.DrawMeshInstancedIndirect(
                mesh, 0, render, new Bounds(Vector3.zero, Vector3.one * 10000f), argsBuffer, 
                0, null, UnityEngine.Rendering.ShadowCastingMode.On, true
            );
        }

        protected void SetupVertexAnim()
        {
            var animations = new Dictionary<string, Material>()
            {
                { "Idle", idle },
                { "Walk", walk }
            };

            var keys = animations.Keys.ToList();
            int count = keys.Count;
            var attributes = new VertexAnimAttribute[count];
            for(int i = 0; i < count; i++)
            {
                var key = keys[i];
                var m = animations[key];
                attributes[i] = VertexAnimHelper.GetAttribute(m, i);
                render.SetTexture("_PosTex_" + key, m.GetTexture(ShaderConst.SHADER_ANIM_TEX));
                render.SetTexture("_NormTex_" + key, m.GetTexture(ShaderConst.SHADER_NORM_TEX));
            }
            attributeBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(VertexAnimAttribute)));
            attributeBuffer.SetData(attributes);
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center.position, gatherHole);
            Gizmos.DrawWireSphere(center.position, gatherDistance);
        }

        protected void OnDestroy()
        {
            crowdBuffer.Dispose();
            argsBuffer.Dispose();
            attributeBuffer.Dispose();
            emitBoundsBuffer.Dispose();
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 18:
                    throttle = knobValue;
                    break;

                case 2:
                    break;
            }
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
            switch (note)
            {
                case 34:
                    useFace = Mathf.Clamp01(1f - useFace);
                    break;

                case 50:
                    useEmission = !useEmission;
                    break;

                case 66:
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/crowd/throttle":
                    throttle = OSCUtils.GetFValue(data, 0);
                    break;

                case "/crowd/mode":
                    mode = (CrowdMode)OSCUtils.GetIValue(data);
                    break;

                case "/crowd/reset":
                    Reset();
                    break;

                case "/crowd/face/use":
                    useFace = OSCUtils.GetFValue(data);
                    break;

                case "/crowd/face/speed":
                    faceScrollSpeed = OSCUtils.GetFValue(data);
                    break;
                    
                case "/crowd/face/random":
                    faceRandom = OSCUtils.GetFValue(data);
                    break;

                case "/crowd/emission/use":
                    useEmission = OSCUtils.GetBoolFlag(data);
                    break;

                case "/crowd/emission/rate":
                    emissionRate = Mathf.Clamp01(OSCUtils.GetFValue(data));
                    break;

                case "/crowd/emission/speed":
                    emissionSpeed = OSCUtils.GetFValue(data);
                    break;

                case "/crowd/emission/random":
                    emissionRandom = OSCUtils.GetFValue(data);
                    break;

            }
        }

    }

}


