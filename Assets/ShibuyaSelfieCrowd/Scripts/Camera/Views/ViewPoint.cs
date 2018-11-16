using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class ViewPoint : MonoBehaviour {

        public bool Fog { get { return fog; } }
        public float FocusDistance { get { return focusDistance; } }

        [SerializeField] protected bool fog = true;
        [SerializeField] protected float focusDistance = 30f;

        void Start () {
        }

        protected void OnDrawGizmos ()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }

    }

}


