using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VJ
{

    [StructLayout(LayoutKind.Sequential)]
    public struct CityVoxelParticle_t {
        public Vector3 position;
        public Vector3 origin;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector2 uv;
        public Vector3 velocity;
        public float speed;
        public float lifetime;
        public uint flow;
    }

}


