using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public abstract class HandyCameraViewBase : CameraViewBase {

        [SerializeField] protected float positionFrequency = 0.5f, rotationFrequency = 0.5f;
        [SerializeField] protected float positionAmount = 1f, rotationAmount = 1f;
        [SerializeField] protected Vector3 positionComponents = new Vector3(1f, 1f, 1f), rotationComponents = new Vector3(1f, 1f, 1f);
        [SerializeField] protected int positionOctave = 3, rotationOctave = 4;
        protected float timePosition, timeRotation;

        protected Vector2[] noiseVectors;

        protected override void Start()
        {
            base.Start();

            timePosition = Random.value * 10f;
            timeRotation = Random.value * 10f;

            noiseVectors = new Vector2[6];
            for(int i = 0; i < 6; i++)
            {
                var theta = Random.value * Mathf.PI * 2f;
                noiseVectors[i].Set(Mathf.Cos(theta), Mathf.Sin(theta));
            }
        }

        protected Vector3 WobblePosition(float dt)
        {
            timePosition += dt * positionFrequency;
            var p = new Vector3(
                Perlin.Fbm(noiseVectors[0] * timePosition, positionOctave),
                Perlin.Fbm(noiseVectors[1] * timePosition, positionOctave),
                Perlin.Fbm(noiseVectors[2] * timePosition, positionOctave)
            );
            return Vector3.Scale(p, positionComponents) * (positionAmount * 2f);
        }

        protected Quaternion WobbleRotation(float dt)
        {
            timeRotation += dt * rotationFrequency;
            var r = new Vector3(
                Perlin.Fbm(noiseVectors[3] * timeRotation, rotationOctave),
                Perlin.Fbm(noiseVectors[4] * timeRotation, rotationOctave),
                Perlin.Fbm(noiseVectors[5] * timeRotation, rotationOctave)
            );
            r = Vector3.Scale(r, rotationComponents) * (rotationAmount * 2f);
            return Quaternion.Euler(r.x, r.y, r.z);
        }

    }

}


