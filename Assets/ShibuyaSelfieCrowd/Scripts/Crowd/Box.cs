using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class Box : MonoBehaviour {

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

    }

}


