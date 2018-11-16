using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VJ
{

    [System.Serializable]
    public class NoiseGen {

        [SerializeField] protected float valueMin, valueMax;
        [SerializeField] protected float scale, offset;

        public float Value(float x, float y)
        {
            return Mathf.Lerp(valueMin, valueMax, Mathf.PerlinNoise(x * scale + offset, y * scale));
        }

    }

}


