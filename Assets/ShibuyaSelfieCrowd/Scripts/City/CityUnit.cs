using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

namespace VJ
{

    [StructLayout (LayoutKind.Sequential)]
    public struct CityAttribute
    {
        public Vector3 wireframe;
        public Vector3 center;
    };

    public enum TileTag
    {
        LD_010_017,
        LD_010_018,
        LD_010_019,
        LD_011_017,
        LD_011_018,
        LD_011_019,
        LD_012_017,
        LD_012_018,
        LD_012_019,
    };

    [RequireComponent (typeof(MeshRenderer), typeof(MeshFilter))]
    public class CityUnit : MonoBehaviour {

        public TileTag Tile { get { return tile; } }
        public Bounds WBounds { get { return bounds; } }
        public Color Emission { get; set; }

        [SerializeField] protected TileTag tile;

        [SerializeField, Range(0f, 10f)] public float noiseScale = 0.1f, noiseIntensity = 5f, noiseOffset = 1f;
        protected Bounds bounds;

        [SerializeField, Range(0.5f, 5f)] public float delta = 5f;

        [SerializeField, Range(0f, 1f)] public float useTexture = 0f;
        protected float _useTexture;

        [SerializeField, Range(0f, 1f)] public float center = 0f;

        [Range(0f, 1f)] public float wireframe = 0f, gain = 1f;
        protected float _wireframe, _gain;

        new public Renderer renderer;
        protected MaterialPropertyBlock block;

        protected ComputeBuffer buffer;

        protected void Awake()
        {
            _useTexture = useTexture;
            _wireframe = wireframe;
            _gain = gain;

            var filter = GetComponent<MeshFilter>();
            var mesh = Build(filter.sharedMesh);
            filter.mesh = mesh;

            renderer = GetComponent<MeshRenderer>();
            block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            bounds = renderer.bounds;

            var vertices = mesh.vertices;
            var len = vertices.Length;
            var attribs = new CityAttribute[len];

            buffer = new ComputeBuffer(len, Marshal.SizeOf(typeof(CityAttribute)));
            for (int i = 0; i < len; i += 3)
            {
                int i0 = i, i1 = i + 1, i2 = i + 2;
                var v0 = vertices[i0];
                var v1 = vertices[i1];
                var v2 = vertices[i2];
                attribs[i0].wireframe = new Vector3(1, 0, 0);
                attribs[i1].wireframe = new Vector3(0, 1, 0);
                attribs[i2].wireframe = new Vector3(0, 0, 1);
                attribs[i0].center = attribs[i1].center = attribs[i2].center = (v0 + v1 + v2) / 3f;
            }
            buffer.SetData(attribs);
            block.SetBuffer("_Attributes", buffer);
            renderer.SetPropertyBlock(block);
        }

        protected void Start () {
        }

        protected void Update()
        {
            var dt = Time.deltaTime * delta;

            _useTexture = Mathf.Lerp(_useTexture, useTexture, dt);
            _wireframe = Mathf.Lerp(_wireframe, wireframe, dt);
            _gain = Mathf.Lerp(_gain, gain, dt);

            block.SetColor("_Emission", Emission);

            block.SetFloat("_UseTexture", _useTexture);
            block.SetFloat("_Center", center);
            block.SetFloat("_Wireframe", _wireframe);
            block.SetFloat("_Gain", _gain);

            block.SetFloat("_NoiseScale", noiseScale);
            block.SetFloat("_NoiseIntensity", noiseIntensity);
            block.SetFloat("_NoiseOffset", noiseOffset);

            block.SetVector("_CaptureBoundsMin", CityGlobal.Instance.WholeBounds.min);
            block.SetVector("_CaptureBoundsMax", CityGlobal.Instance.WholeBounds.max);
            block.SetTexture("_CaptureDepth", CityGlobal.Instance.OrthographicDepth);

            renderer.SetPropertyBlock(block);
        }

        protected void OnDestroy()
        {
            buffer.Dispose();
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        protected Mesh Build(Mesh source)
        {
            var inVertices = source.vertices;
            var inNormals = source.normals;
            var inTexcoords = source.uv;
            var inTriangles = source.triangles;

            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texcoords = new List<Vector2>();
            var triangles = new List<int>();

            for (int i = 0, n = inTriangles.Length; i < n; i += 3)
            {
                int i0 = inTriangles[i], i1 = inTriangles[i + 1], i2 = inTriangles[i + 2];

                vertices.Add(inVertices[i0]); vertices.Add(inVertices[i1]); vertices.Add(inVertices[i2]);
                normals.Add(inNormals[i0]); normals.Add(inNormals[i1]); normals.Add(inNormals[i2]);
                texcoords.Add(inTexcoords[i0]); texcoords.Add(inTexcoords[i1]); texcoords.Add(inTexcoords[i2]);
                triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
            }

            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, texcoords);
            mesh.SetTriangles(triangles.ToArray(), 0);
            // mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

    }

}


