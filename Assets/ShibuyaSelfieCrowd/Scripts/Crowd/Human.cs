using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace VJ
{

    [StructLayout (LayoutKind.Sequential)]
    public struct Human 
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 velocity;
        public float lifetime;
        public uint alive;
    };

}


