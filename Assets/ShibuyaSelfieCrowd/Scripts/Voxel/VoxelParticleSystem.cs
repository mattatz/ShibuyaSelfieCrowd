using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

using VoxelSystem;

namespace VJ
{

    #region Enums

    public enum ParticleMode
    {
        Immediate = 0,
        Delay = 1,
        Flow = 2,
        Transform = 3,
        Clip = 4,
    };

    public enum VoxelMode
    {
        Default,
        Glitch,
        Clip
    };

    #endregion

    public class VoxelParticleSystem : ControllableBase {

        #region Accessors

        public ParticleMode PMode {
            get { return particleMode; }
            set {
                particleMode = value;
            }
        }

        public VoxelMode VMode {
            get { return voxelMode; }
            set {
                voxelMode = value;
                switch(voxelMode)
                {
                    case VoxelMode.Glitch:
                        Glitch();
                        break;
                }
            }
        }

        public Bounds BaseClipBounds {
            get { return baseClipBounds; }
        }

        public Bounds ClipBounds {
            get { return clipBounds; }
            set { clipBounds = value; }
        }

        #endregion

        [SerializeField] protected SkinnedMeshRenderer skin;
        [SerializeField] protected int resolution = 128;
        [SerializeField, Range(0, 5)] protected int level = 0;
        [SerializeField] protected int count = 262144;
        [SerializeField] protected ComputeShader voxelizer, voxelControl, particleUpdate;

        [SerializeField] protected ParticleMode particleMode = ParticleMode.Immediate;
        [SerializeField] protected VoxelMode voxelMode = VoxelMode.Default;
        [SerializeField] protected bool voxelVisible = true;

        [SerializeField, Range(0f, 0.5f)] protected float edge = 0.5f;
        protected float _edge;

        [SerializeField] protected Color emission = Color.black;
        [SerializeField] protected GradientTextureGen gradGen;
        [SerializeField, Range(0.01f, 30f)] protected float gradientScale = 5f, gradientSpeed = 0.1f;
        [SerializeField] protected bool useColor;
        protected Color _emission;

        [SerializeField] protected List<GradientTextureGen> texGen;
        protected Texture2D gradient;
        [SerializeField] protected bool useEmission = true;
        [SerializeField, Range(0f, 0.5f)] protected float emissionRate = 0.01f;
        [SerializeField, Range(0f, 1.5f)] protected float emissionRandom = 0.01f;
        [SerializeField, Range(0f, 15.0f)] protected float emissionSpeed = 1.0f;
        protected float emissionOffset = 0f;

        protected Dictionary<ParticleMode, Kernel> particleKernels;
        protected Dictionary<VoxelMode, Kernel> voxelKernels;
        protected Kernel gradKer;

        #region Particle properties

        [SerializeField] protected float speedScaleMin = 2.0f, speedScaleMax = 5.0f;
        [SerializeField] protected float speedLimit = 1.0f;
        [SerializeField, Range(0f, 3f)] protected float drag = 0.1f;
        [SerializeField] protected Vector3 gravity = Vector3.zero;
        [SerializeField] protected float speedToSpin = 60.0f;
        [SerializeField] protected float maxSpin = 20.0f;
        [SerializeField] protected float noiseAmplitude = 1.0f;
        [SerializeField] protected float noiseFrequency = 0.01f;
        [SerializeField] protected float noiseMotion = 1.0f;
        protected Vector3 noiseOffset;

        #endregion

        [SerializeField, Range(1, 10)] protected int frame = 1;
        [SerializeField, Range(0, 50f)] protected float delaySpeed = 1.5f, transformSpeed = 1.5f, clipSpeed = 1.5f;
        [SerializeField, Range(0, 1f)] protected float flowSpeed = 0.1f;
        [SerializeField] protected Bounds baseClipBounds, clipBounds;

        #region Flow Random properties

        [SerializeField] protected bool flowRandom = false;
        [SerializeField] protected int flowRandomFreq = 100;

        #endregion

        #region Voxel control properties

        [SerializeField, Range(0f, 1f)] protected float throttle = 0.1f;

        #endregion

        protected Mesh cached;
        protected GPUVoxelData data;
        protected Bounds bounds;

        protected Kernel flowRandomKer;
        protected Kernel randomizeKer, glitchKer;

        protected ComputeBuffer particleBuffer;

        protected new Renderer renderer;
        protected MaterialPropertyBlock block;

        #region Shader property keys

        protected const string kVoxelBufferKey = "_VoxelBuffer", kVoxelCountKey = "_VoxelCount";
        protected const string kParticleBufferKey = "_ParticleBuffer", kParticleCountKey = "_ParticleCount";
		protected const string kStartKey = "_Start", kEndKey = "_End", kSizeKey = "_Size";
        protected const string kWidthKey = "_Width", kHeightKey = "_Height", kDepthKey = "_Depth";
        protected const string kLevelKey = "_Level";
        protected const string kUnitLengthKey = "_UnitLength", kInvUnitLengthKey = "_InvUnitLength", kHalfUnitLengthKey = "_HalfUnitLength";

        protected const string kTimeKey = "_Time", kDTKey = "_DT";
        protected const string kSpeedKey = "_Speed";
        protected const string kDamperKey = "_Damper";
        protected const string kGravityKey = "_Gravity";
        protected const string kSpinKey = "_Spin";
        protected const string kNoiseParamsKey = "_NoiseParams", kNoiseOffsetKey = "_NoiseOffset";
        protected const string kThresholdKey = "_Threshold";
        protected const string kThrottleKey = "_Throttle";
        protected const string kDelaySpeedKey = "_DelaySpeed", kTransformSpeedKey = "_TransformSpeed", kClipSpeedKey = "_ClipSpeed";
        protected const string kFlowSpeedKey = "_FlowSpeed";
        protected const string kClipMinKey = "_ClipMin", kClipMaxKey = "_ClipMax";

        #endregion

        #region MonoBehaviour functions

        void Start () {
            _edge = edge;

            block = new MaterialPropertyBlock();
            renderer = GetComponent<Renderer>();
            renderer.GetPropertyBlock(block);

            _emission = emission;

            flowRandomKer = new Kernel(particleUpdate, "FlowRandom");
            SetupParticleKernels();
            SetupVoxelKernels();

            gradKer = new Kernel(particleUpdate, "Gradient");
            particleUpdate.SetTexture(gradKer.Index, "_Gradient", gradGen.Create(128, 1));
        }
     
        protected void Update () {
            if(Time.frameCount % frame == 0)
            {
                cached = Sample();
                // bounds = cached.bounds;
                bounds.Encapsulate(cached.bounds.min);
                bounds.Encapsulate(cached.bounds.max);
                Voxelize(cached, bounds);
            }

            // if(flowRandom && Time.frameCount % flowRandomFreq == 0) FlowRandom();

            var dt = Time.deltaTime;

            if(voxelMode != VoxelMode.Default) {
                ComputeVoxel(voxelKernels[voxelMode], dt);
            }

            ComputeParticle(particleKernels[particleMode], dt);

            particleUpdate.SetFloat("_UseColor", useColor ? 1f : 0f);
            particleUpdate.SetFloat("_GradientScale", gradientScale);
            particleUpdate.SetFloat("_GradientSpeed", gradientSpeed);
            ComputeParticle(gradKer, dt);

            _emission = Color.Lerp(_emission, emission, dt);
            _edge = Mathf.Lerp(_edge, edge, dt);
            emissionOffset += dt * emissionSpeed;

            block.SetBuffer(kParticleBufferKey, particleBuffer);
            block.SetFloat("_EmissionRate", emissionRate * (useEmission ? 1f : 0f));
            block.SetTexture("_Gradient", gradient);
            block.SetColor("_EmissionBase", _emission);
            block.SetFloat("_EmissionOffset", emissionOffset);
            block.SetFloat("_EmissionRandom", emissionRandom);

            block.SetFloat("_Edge", _edge);

            renderer.SetPropertyBlock(block);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        void OnDestroy ()
        {
            ReleaseBuffers();
        }

        protected void OnEnable()
        {
            cached = Sample();
            bounds = cached.bounds;
            Voxelize(cached, bounds);

            var pointMesh = BuildPoints(count);
            GetComponent<MeshFilter>().sharedMesh = pointMesh;
            particleBuffer = new ComputeBuffer(pointMesh.vertexCount, Marshal.SizeOf(typeof(VoxelParticle_t)));
            particleBuffer.SetData(new VoxelParticle_t[pointMesh.vertexCount]);
            Setup();

            UpdateGradient(0);
        }

        protected void OnDisable()
        {
            ReleaseBuffers();
        }

        protected void UpdateGradient(int index = 0)
        {
            if(gradient != null)
            {
                Destroy(gradient);
            }
            gradient = texGen[index % texGen.Count].Create(128, 1);
        }

        protected void ReleaseBuffers()
        {
            if(data != null)
            {
                data.Dispose();
                data = null;
            }

            if(particleBuffer != null)
            {
                particleBuffer.Release();
                particleBuffer = null;
            }

            var filter = GetComponent<MeshFilter>();
            if(filter.sharedMesh != null)
            {
                Destroy(filter.sharedMesh);
            }

            if(gradient != null)
            {
                Destroy(gradient);
            }
        }

        #endregion

        #region Initialization

        void Setup()
        {
            var setupKer = new Kernel(particleUpdate, "Setup");
            particleUpdate.SetBuffer(setupKer.Index, kVoxelBufferKey, data.Buffer);
            particleUpdate.SetInt(kVoxelCountKey, data.Width * data.Height * data.Depth);
            particleUpdate.SetInt(kWidthKey, data.Width);
            particleUpdate.SetInt(kHeightKey, data.Height);
            particleUpdate.SetInt(kLevelKey, level);
            particleUpdate.SetInt(kDepthKey, data.Depth);

            particleUpdate.SetBuffer(setupKer.Index, kParticleBufferKey, particleBuffer);
            particleUpdate.SetInt(kParticleCountKey, particleBuffer.count);

            particleUpdate.SetVector(kSpeedKey, new Vector2(speedScaleMin, speedScaleMax));
            particleUpdate.SetFloat(kUnitLengthKey, data.UnitLength);

            particleUpdate.Dispatch(setupKer.Index, particleBuffer.count / (int)setupKer.ThreadX + 1, (int)setupKer.ThreadY, (int)setupKer.ThreadZ);
        }

        Mesh BuildPoints(int count)
        {
            var indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;

            var mesh = new Mesh();
			mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = new Vector3[count];
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.RecalculateBounds();
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
            return mesh;
        }

        void SetupParticleKernels()
        {
            particleKernels = new Dictionary<ParticleMode, Kernel>();
            foreach(ParticleMode mode in Enum.GetValues(typeof(ParticleMode)))
            {
                particleKernels.Add(mode, new Kernel(particleUpdate, Enum.GetName(typeof(ParticleMode), mode)));
            }
        }

        void SetupVoxelKernels()
        {
            voxelKernels = new Dictionary<VoxelMode, Kernel>();
            foreach(VoxelMode mode in Enum.GetValues(typeof(VoxelMode)))
            {
                voxelKernels.Add(mode, new Kernel(voxelControl, Enum.GetName(typeof(VoxelMode), mode)));
            }
        }
 

        #endregion

        Vector4 GetTime(float t)
        {
            return new Vector4(t / 4f, t, t * 2f, t * 3f);
        }

        public void Voxelize(Mesh mesh, Bounds bounds)
        {
            if(data != null)
            {
                data.Dispose();
                data = null;
            }

			// data = GPUVoxelizer.Voxelize(voxelizer, mesh, mesh.bounds, resolution, true, false);
			data = GPUVoxelizer.Voxelize(voxelizer, mesh, Mathf.Max(10, resolution) >> level, true, false);
        }

        Mesh Sample()
        {
            var mesh = new Mesh();
            skin.BakeMesh(mesh);
            return mesh;
        }

        void ComputeVoxel (Kernel kernel, float dt)
        {
            voxelControl.SetBuffer(kernel.Index, kVoxelBufferKey, data.Buffer);

			voxelControl.SetVector(kStartKey, bounds.min);
			voxelControl.SetVector(kEndKey, bounds.max);
			voxelControl.SetVector(kSizeKey, bounds.size);

            voxelControl.SetFloat(kUnitLengthKey, data.UnitLength);
			voxelControl.SetFloat(kInvUnitLengthKey, 1f / data.UnitLength);
			voxelControl.SetFloat(kHalfUnitLengthKey, data.UnitLength * 0.5f);

            voxelControl.SetInt(kWidthKey, data.Width);
            voxelControl.SetInt(kHeightKey, data.Height);
            voxelControl.SetInt(kDepthKey, data.Depth);

            voxelControl.SetVector(kTimeKey, GetTime(Time.timeSinceLevelLoad));
            voxelControl.SetFloat(kDTKey, dt);

            voxelControl.SetFloat(kThrottleKey, throttle);

            GPUHelper.Dispatch2D(voxelControl, kernel, data.Width, data.Height);
        }

        void ComputeParticle (Kernel kernel, float dt = 0f)
        {
            particleUpdate.SetBuffer(kernel.Index, kVoxelBufferKey, data.Buffer);
            particleUpdate.SetInt(kVoxelCountKey, data.Width * data.Height * data.Depth);
            particleUpdate.SetInt(kWidthKey, data.Width);
            particleUpdate.SetInt(kHeightKey, data.Height);
            particleUpdate.SetInt(kDepthKey, data.Depth);
            particleUpdate.SetInt(kLevelKey, level);
            particleUpdate.SetFloat(kUnitLengthKey, data.UnitLength);

            particleUpdate.SetBuffer(kernel.Index, kParticleBufferKey, particleBuffer);
            particleUpdate.SetInt(kParticleCountKey, particleBuffer.count);

            particleUpdate.SetVector(kTimeKey, GetTime(Time.timeSinceLevelLoad));
            particleUpdate.SetVector(kDTKey, new Vector2(dt, 1f / dt));
            particleUpdate.SetVector(kDamperKey, new Vector2(Mathf.Exp(-drag * dt), speedLimit));
            particleUpdate.SetVector(kGravityKey, gravity * dt);

            var pi360dt = Mathf.PI * dt / 360;
            particleUpdate.SetVector(kSpinKey, new Vector2(maxSpin * pi360dt, speedToSpin * pi360dt));

            particleUpdate.SetVector(kNoiseParamsKey, new Vector2(noiseFrequency, noiseAmplitude * dt));

            var noiseDir = (gravity == Vector3.zero) ? Vector3.up : gravity.normalized;
            noiseOffset += noiseDir * noiseMotion * dt;
            particleUpdate.SetVector(kNoiseOffsetKey, noiseOffset);

            particleUpdate.SetFloat(kDelaySpeedKey, delaySpeed);
            particleUpdate.SetFloat(kTransformSpeedKey, transformSpeed);
            particleUpdate.SetFloat(kClipSpeedKey, clipSpeed);
            particleUpdate.SetVector(kClipMinKey, clipBounds.min);
            particleUpdate.SetVector(kClipMaxKey, clipBounds.max);

            particleUpdate.SetFloat(kFlowSpeedKey, flowSpeed);

            particleUpdate.Dispatch(kernel.Index, particleBuffer.count / (int)kernel.ThreadX + 1, (int)kernel.ThreadY, (int)kernel.ThreadZ);
        }

        public void Randomize()
        {
            voxelControl.SetFloat("_Seed", Random.value);
            // Voxelize(cached, bounds);
            // ComputeVoxel(voxelKernels[VoxelMode.Randomize], 0f);
        }

        public void Glitch()
        {
            voxelControl.SetFloat("_Seed", Random.value);
            // Voxelize(cached, bounds);
            // ComputeVoxel(voxelKernels[VoxelMode.Glitch], 0f);
        }

        public void Clip(Vector3 min, Vector3 max)
        {
            voxelControl.SetVector(kClipMinKey, new Vector3(min.x * data.Width, min.y * data.Height, min.z * data.Depth));
            voxelControl.SetVector(kClipMaxKey, new Vector3(max.x * data.Width, max.y * data.Height, max.z * data.Depth));
            // ComputeVoxel(voxelKernels[VoxelMode.Clip], 0f);
            VMode = VoxelMode.Clip;
        }

        public void FlowRandom(float throttle = 0.1f)
        {
            particleUpdate.SetFloat("_FlowRandomThrottle", throttle);
            ComputeParticle(flowRandomKer);
        }

        public override void NoteOn(int note)
        {
            switch(note)
            {
                case 33:
                    FlowRandom(1f);
                    break;
                case 49:
                    break;
                case 65:
                    break;

                case 34:
                    voxelVisible = !voxelVisible;
                    break;
                case 50:
                    break;
                case 66:
                    break;
            }
        }

        public override void NoteOff(int note)
        {
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 17:
                    emission = Color.Lerp(Color.black, Color.white, knobValue);
                    break;
                case 1:
                    level = Mathf.FloorToInt(knobValue * 4);
                    break;

                case 20:
                    edge = Mathf.Lerp(0f, 0.5f, knobValue);
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/voxel/visible":
                    voxelVisible = OSCUtils.GetBoolFlag(data);
                    break;

                case "/voxel/particle/mode":
                    PMode = (ParticleMode)OSCUtils.GetIValue(data);
                    break;

                case "/voxel/control/mode":
                    VMode = (VoxelMode)OSCUtils.GetIValue(data);
                    break;

                case "/voxel/flow":
                    FlowRandom(OSCUtils.GetFValue(data, 1));
                    break;

                case "/voxel/emission/animate":
                    useEmission = OSCUtils.GetBoolFlag(data);
                    break;
                case "/voxel/emission/gradient":
                    UpdateGradient(OSCUtils.GetIValue(data, 0, 0));
                    break;
                case "/voxel/emission/speed":
                    emissionSpeed = OSCUtils.GetFValue(data);
                    break;
                case "/voxel/emission/rate":
                    emissionRate = OSCUtils.GetFValue(data);
                    break;
                case "/voxel/emission/random":
                    emissionRandom = OSCUtils.GetFValue(data);
                    break;

            }
        }

    }

}


