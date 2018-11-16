using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

using VoxelSystem;

namespace VJ
{

    public enum CityVoxelMode
    {
        Default = 0,
        Depth = 1,
        Floor = 2,
    };

    public class CityVoxelParticleSystem : MonoBehaviour
    {
        public TileTag tileTag;
        public Mesh mesh;

        [SerializeField] protected int resolution = 128;
        [Range(0, 5)] public int level = 0;
        protected int _level;

        [SerializeField] protected int count = 262144;
        [SerializeField] protected ComputeShader voxelizer, voxelControl, particleUpdate;

        public ParticleMode particleMode = ParticleMode.Immediate;
        public CityVoxelMode voxelMode = CityVoxelMode.Default;
        [SerializeField] protected bool volume = true, voxelVisible = true;

        [SerializeField] protected float unitLength = 1f;

        [Range(0f, 1.0f)] public float size = 1.0f;
        protected float _size;

        [Range(0f, 0.5f)] public float edge = 0.5f;
        protected float _edge;

        [Range(0f, 1f)] public float useCapture = 0f;
        protected float _useCapture;

        [Range(0f, 1f)] public float usePack = 0f;
        protected float _usePack;

        [ColorUsage(false, true)] public Color emission = Color.black;
        protected Color _emission;

        public bool useEmission = true;
        [Range(0f, 0.5f)] public float emissionRate = 0.01f;
        [Range(0f, 1.5f)] public float emissionRandom = 0.01f;
        public float emissionOffset = 0f;

        protected Dictionary<ParticleMode, Kernel> particleKernels;
        protected Kernel gradKer;

        #region Particle properties

        public float speedScaleMin = 2.0f, speedScaleMax = 5.0f;
        public float speedLimit = 1.0f;
        [Range(0f, 3f)] public float drag = 0.1f;
        public Vector3 gravity = Vector3.zero;
        public float speedToSpin = 60.0f;
        public float maxSpin = 20.0f;
        public float noiseAmplitude = 1.0f;
        public float noiseFrequency = 0.01f;
        public float noiseMotion = 1.0f;
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
        public int gridResolution = 256;
        protected float gridUnitLength;

        public Vector2 tileOffset;

        #endregion

        protected GPUVoxelData data;

        [SerializeField] protected float margin = 10f;
        public Bounds bounds;

        protected Kernel flowRandomKer;
        protected Kernel randomizeKer, glitchKer;

        protected ComputeBuffer particleBuffer;

        [SerializeField] protected Material material;
        protected Mesh pointMesh;
        public MaterialPropertyBlock block;

        #region Shader property keys

        protected const string kVoxelBufferKey = "_VoxelBuffer", kVoxelCountKey = "_VoxelCount";
        protected const string kParticleBufferKey = "_ParticleBuffer", kParticleCountKey = "_ParticleCount";

        protected const string kSpeedKey = "_Speed";
        protected const string kThresholdKey = "_Threshold";
        protected const string kDelaySpeedKey = "_DelaySpeed", kTransformSpeedKey = "_TransformSpeed", kClipSpeedKey = "_ClipSpeed";
        protected const string kFlowSpeedKey = "_FlowSpeed";
        protected const string kClipMinKey = "_ClipMin", kClipMaxKey = "_ClipMax";

        #endregion

        #region MonoBehaviour functions

        protected void Start () {
            _size = size;
            _edge = edge;
            _useCapture = useCapture;
            _usePack = usePack;

            Initialize();

            _emission = emission;

            flowRandomKer = new Kernel(particleUpdate, "FlowRandom");
            SetupParticleKernels();
        }
     
        protected void Update () {
            var dt = Time.deltaTime;

            _size = Mathf.Lerp(_size, size, dt);
            _edge = Mathf.Lerp(_edge, edge, dt);
            _useCapture = Mathf.Lerp(_useCapture, useCapture, dt);
            _usePack = Mathf.Lerp(_usePack, usePack, dt);

            if (_size <= 0f) return;

            if(_level != level)
            {
                _level = level;
                Voxelize(mesh, bounds);
            }

            if(flowRandom && Time.frameCount % flowRandomFreq == 0) FlowRandom();

            switch(voxelMode)
            {
                case CityVoxelMode.Default:
                    unitLength = Mathf.Lerp(unitLength, data.UnitLength, dt);
                    break;
                case CityVoxelMode.Depth:
                    DepthKernel(dt);
                    unitLength = Mathf.Lerp(unitLength, data.UnitLength, dt);
                    break;
                case CityVoxelMode.Floor:
                    FloorKernel(dt);
                    unitLength = Mathf.Lerp(unitLength, gridUnitLength, dt);
                    break;
            }

            ComputeParticleKernel(particleKernels[particleMode], dt);

            _emission = Color.Lerp(_emission, emission, dt);

            block.SetBuffer(kParticleBufferKey, particleBuffer);
            block.SetFloat("_EmissionRate", emissionRate * (useEmission ? 1f : 0f));
            block.SetColor("_EmissionBase", _emission);
            block.SetFloat("_EmissionOffset", emissionOffset);
            block.SetFloat("_EmissionRandom", emissionRandom);

            block.SetFloat("_Size", _size);
            block.SetFloat("_Edge", _edge);
            block.SetFloat("_UseCapture", _useCapture);
            block.SetFloat("_UsePack", _usePack);

            block.SetTexture("_CaptureTex", CityGlobal.Instance.OrthographicColor);
            block.SetVector("_CaptureBoundsMin", CityGlobal.Instance.WholeBounds.min);
            block.SetVector("_CaptureBoundsSize", CityGlobal.Instance.WholeBounds.size);
            block.SetVector("_TileOffset", tileOffset);

            Graphics.DrawMesh(pointMesh, transform.localToWorldMatrix, material, gameObject.layer, null, 0, block);
        }

        protected Bounds BoundsWorldToLocal(Bounds wbounds, Transform tr)
        {
            var wmin = wbounds.min;
            var wmax = wbounds.max;

            var p1 = tr.InverseTransformPoint(new Vector3(wmin.x, wmin.y, wmin.z));
            var p2 = tr.InverseTransformPoint(new Vector3(wmin.x, wmin.y, wmax.z));
            var p3 = tr.InverseTransformPoint(new Vector3(wmin.x, wmax.y, wmax.z));
            var p4 = tr.InverseTransformPoint(new Vector3(wmin.x, wmax.y, wmin.z));
            var p5 = tr.InverseTransformPoint(new Vector3(wmax.x, wmin.y, wmin.z));
            var p6 = tr.InverseTransformPoint(new Vector3(wmax.x, wmin.y, wmax.z));
            var p7 = tr.InverseTransformPoint(new Vector3(wmax.x, wmax.y, wmax.z));
            var p8 = tr.InverseTransformPoint(new Vector3(wmax.x, wmax.y, wmin.z));

            var b = new Bounds(p1, Vector3.zero);
            b.Encapsulate(p2);
            b.Encapsulate(p3);
            b.Encapsulate(p4);
            b.Encapsulate(p5);
            b.Encapsulate(p6);
            b.Encapsulate(p7);
            b.Encapsulate(p8);
            return b;
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

        protected void Initialize()
        {
            _level = level;

            if(bounds.size.magnitude <= float.Epsilon)
            {
                bounds = mesh.bounds;
            }

            var min = bounds.min;
            var max = bounds.max;
            min.x += margin; min.y += margin;
            max.x -= margin; max.y -= margin;
            bounds.SetMinMax(min, max);

            Voxelize(mesh, bounds);

            pointMesh = BuildPoints(count, mesh.bounds);
            // GetComponent<MeshFilter>().sharedMesh = pointMesh;
            particleBuffer = new ComputeBuffer(pointMesh.vertexCount, Marshal.SizeOf(typeof(CityVoxelParticle_t)));
            particleBuffer.SetData(new CityVoxelParticle_t[pointMesh.vertexCount]);
            SetupKernel();
        }

        protected void OnEnable()
        {
            block = new MaterialPropertyBlock();
        }

        protected void OnDisable()
        {
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
        }

        #endregion

        #region Initialization

        void SetupKernel()
        {
            var setupKer = new Kernel(particleUpdate, "Setup");
            particleUpdate.SetBuffer(setupKer.Index, kVoxelBufferKey, data.Buffer);
            particleUpdate.SetInt(kVoxelCountKey, data.Width * data.Height * data.Depth);
            particleUpdate.SetInt("_Width", data.Width);
            particleUpdate.SetInt("_Height", data.Height);
            particleUpdate.SetInt("_Depth", data.Depth);
            particleUpdate.SetInt("_Level", level);

            particleUpdate.SetBuffer(setupKer.Index, kParticleBufferKey, particleBuffer);
            particleUpdate.SetInt(kParticleCountKey, particleBuffer.count);

            particleUpdate.SetVector(kSpeedKey, new Vector2(speedScaleMin, speedScaleMax));
            particleUpdate.SetFloat("_UnitLength", unitLength);

            GPUHelper.Dispatch1D(particleUpdate, setupKer, particleBuffer.count);
        }

        Mesh BuildPoints(int count, Bounds bounds)
        {
            var indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;

            var mesh = new Mesh();
			mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = new Vector3[count];
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            // mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            mesh.bounds = bounds;
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


        #endregion

        Vector4 GetTime(float t)
        {
            return new Vector4(t / 4f, t, t * 2f, t * 3f);
        }

        public void Voxelize(Bounds bounds)
        {
            Voxelize(mesh, bounds);
        }

        public void Voxelize(Mesh mesh, Bounds bounds)
        {
            if(data != null)
            {
                data.Dispose();
                data = null;
            }

			// data = GPUVoxelizer.Voxelize(voxelizer, mesh, mesh.bounds, resolution, true, false);
			data = GPUVoxelizer.Voxelize(
                voxelizer, mesh, bounds, Mathf.Max(10, resolution) >> _level, 
                volume, false
            );
        }

        protected void SetupControlKernel(Kernel ker, float dt)
        {
            voxelControl.SetBuffer(ker.Index, kVoxelBufferKey, data.Buffer);

			voxelControl.SetVector("_Start", bounds.min);
			voxelControl.SetVector("_End", bounds.max);
			voxelControl.SetVector("_Size", bounds.size);

            voxelControl.SetFloat("_UnitLength", data.UnitLength);
			voxelControl.SetFloat("_InvUnitLength", 1f / data.UnitLength);
			voxelControl.SetFloat("_HalfUnitLength", data.UnitLength * 0.5f);

            voxelControl.SetInt("_Width", data.Width);
            voxelControl.SetInt("_Height", data.Height);
            voxelControl.SetInt("_Depth", data.Depth);

            voxelControl.SetVector("_Time", GetTime(Time.timeSinceLevelLoad));
            voxelControl.SetFloat("_DT", dt);

            voxelControl.SetFloat("_Throttle", throttle);

            voxelControl.SetTexture(ker.Index, "_CaptureDepth", CityGlobal.Instance.OrthographicDepth);
            voxelControl.SetVector("_CaptureBoundsMin", CityGlobal.Instance.WholeBounds.min);
            voxelControl.SetVector("_CaptureBoundsMax", CityGlobal.Instance.WholeBounds.max);

            voxelControl.SetVector("_TileOffset", tileOffset);

            var instance = CityGlobal.Instance;
            if(instance.Tiles.ContainsKey(tileTag)) {
                var u = instance.Tiles[tileTag];
                var tb = BoundsWorldToLocal(u.WBounds, transform);
                voxelControl.SetVector("_TileBoundsMin", tb.min);
                voxelControl.SetVector("_TileBoundsMax", tb.max);
                gridUnitLength = (tb.max.x - tb.min.x) / gridResolution;

                var wb = BoundsWorldToLocal(instance.WholeBounds, transform);
                voxelControl.SetVector("_WholeBoundsMin", wb.min);
                voxelControl.SetVector("_WholeBoundsMax", wb.max);

                // uv (xy : scale, wz : offset)
                voxelControl.SetVector("_Tile", new Vector4(tb.size.x / wb.size.x, tb.size.y / wb.size.y, (tb.min.x - wb.min.x) / wb.size.x, (tb.min.y - wb.min.y) / wb.size.y));
            }

        }

        protected void DepthKernel (float dt)
        {
            var kernel = new Kernel(voxelControl, "Depth");
            SetupControlKernel(kernel, dt);

            voxelControl.SetVector("_TileOffset", tileOffset);

            var instance = CityGlobal.Instance;
            if(instance.Tiles.ContainsKey(tileTag)) {
                var u = instance.Tiles[tileTag];
                var tb = BoundsWorldToLocal(u.WBounds, transform);
                voxelControl.SetVector("_TileBoundsMin", tb.min);
                voxelControl.SetVector("_TileBoundsMax", tb.max);

                var wb = BoundsWorldToLocal(instance.WholeBounds, transform);
                voxelControl.SetVector("_WholeBoundsMin", wb.min);
                voxelControl.SetVector("_WholeBoundsMax", wb.max);

                // uv (xy : scale, wz : offset)
                voxelControl.SetVector(
                    "_Tile",
                    new Vector4(
                        tb.size.x / wb.size.x, 
                        tb.size.y / wb.size.y, 
                        (tb.min.x - wb.min.x) / wb.size.x, 
                        (tb.min.y - wb.min.y) / wb.size.y
                    )
                );
            }

            GPUHelper.Dispatch3D(voxelControl, kernel, data.Width, data.Height, data.Depth);
        }

        protected void FloorKernel (float dt)
        {
            gridUnitLength = 0f;

            var kernel = new Kernel(voxelControl, "FloorGrid");

            SetupControlKernel(kernel, dt);

            voxelControl.SetInt("_GridResolution", gridResolution);
            GPUHelper.Dispatch1D(voxelControl, kernel, data.Buffer.count);
        }

        protected void ComputeParticleKernel (Kernel kernel, float dt = 0f)
        {
            particleUpdate.SetBuffer(kernel.Index, kVoxelBufferKey, data.Buffer);
            particleUpdate.SetInt(kVoxelCountKey, data.Width * data.Height * data.Depth);
            particleUpdate.SetInt("_Width", data.Width);
            particleUpdate.SetInt("_Height", data.Height);
            particleUpdate.SetInt("_Depth", data.Depth);
            particleUpdate.SetInt("_Level", level);
            particleUpdate.SetFloat("_UnitLength", unitLength);

            particleUpdate.SetBuffer(kernel.Index, kParticleBufferKey, particleBuffer);
            particleUpdate.SetInt(kParticleCountKey, particleBuffer.count);

            particleUpdate.SetVector("_Time", GetTime(Time.timeSinceLevelLoad));
            particleUpdate.SetVector("_DT", new Vector2(dt, 1f / dt));
            particleUpdate.SetVector("_Damper", new Vector2(Mathf.Exp(-drag * dt), speedLimit));
            particleUpdate.SetVector("_Gravity", gravity * dt);

            var pi360dt = Mathf.PI * dt / 360;
            particleUpdate.SetVector("_Spin", new Vector2(maxSpin * pi360dt, speedToSpin * pi360dt));

            particleUpdate.SetVector("_NoiseParams", new Vector2(noiseFrequency, noiseAmplitude * dt));

            var noiseDir = (gravity == Vector3.zero) ? Vector3.up : gravity.normalized;
            noiseOffset += noiseDir * noiseMotion * dt;
            particleUpdate.SetVector("_NoiseOffset", noiseOffset);

            particleUpdate.SetFloat(kDelaySpeedKey, delaySpeed);
            particleUpdate.SetFloat(kTransformSpeedKey, transformSpeed);
            particleUpdate.SetFloat(kClipSpeedKey, clipSpeed);
            particleUpdate.SetVector(kClipMinKey, clipBounds.min);
            particleUpdate.SetVector(kClipMaxKey, clipBounds.max);

            particleUpdate.SetFloat(kFlowSpeedKey, flowSpeed);

            GPUHelper.Dispatch1D(particleUpdate, kernel, particleBuffer.count);
        }

        public void Randomize()
        {
            voxelControl.SetFloat("_Seed", Random.value);
            // ComputeVoxel(voxelKernels[VoxelMode.Randomize], 0f);
        }

        public void FlowRandom(float throttle = 0.1f)
        {
            particleUpdate.SetFloat("_FlowRandomThrottle", throttle);
            ComputeParticleKernel(flowRandomKer);
        }

    }

}


