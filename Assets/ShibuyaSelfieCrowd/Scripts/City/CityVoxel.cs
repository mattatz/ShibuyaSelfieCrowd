using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

namespace VJ
{

    [System.Serializable]
    public class CityVoxelUnit
    {
        public TileTag tileTag;
        public Mesh mesh;
        public Texture2D texture;
        public Bounds bounds;
    };

    public class CityVoxel : ControllableBase {

        [SerializeField] protected GameObject prefab;
        [SerializeField] protected List<CityVoxelUnit> units;
        protected List<CityVoxelParticleSystem> voxels;

        [SerializeField] protected ParticleMode particleMode = ParticleMode.Delay;
        [SerializeField] protected CityVoxelMode voxelMode = CityVoxelMode.Default;

        [SerializeField, Range(0, 3)] protected int level = 0;
        [SerializeField, Range(0f, 1.0f)] protected float size = 1.0f;
        [SerializeField, Range(0f, 0.5f)] protected float edge = 0.5f;
        [SerializeField, Range(0f, 1f)] protected float useCapture = 0f, usePack = 0f;

        [SerializeField] protected bool resolutionReactive = false;
        [SerializeField, Range(8, 80)] protected int gridResolution = 64;

        [SerializeField] protected bool tileReactive = false;
        [SerializeField] protected float tileDuration = 0.05f;
        [SerializeField, Range(-0.5f, 0.5f)] protected float tileSpeedX = 0f, tileSpeedY = 0.1f;
        protected Vector2 tileOffset;

        [SerializeField] protected bool useEmission = true;
        [SerializeField, Range(0f, 0.5f)] protected float emissionRate = 0.01f;
        [SerializeField, Range(0f, 1.5f)] protected float emissionRandom = 0.01f;
        [SerializeField, Range(0f, 15.0f)] protected float emissionSpeed = 1.0f;
        [SerializeField] protected float emissionOffset = 0f;

        protected Coroutine iTrigger;
        protected Action callback;

        protected void Awake () {
            voxels = units.Select(u =>
            {
                var go = Instantiate(prefab);
                go.name = u.tileTag.ToString();
                go.transform.SetParent(transform, false);

                var v = go.GetComponent<CityVoxelParticleSystem>();
                v.tileTag = u.tileTag;
                v.mesh = u.mesh;
                v.block.SetTexture("_MainTex", u.texture);

                if(u.bounds.size.magnitude >= float.Epsilon)
                {
                    v.bounds = u.bounds;
                }

                return v;
            }).ToList();
        }

        protected void Update() {
            var dt = Time.deltaTime;

            tileOffset.x += dt * tileSpeedX;
            tileOffset.y += dt * tileSpeedY;
            emissionOffset += dt * emissionSpeed;

            voxels.ForEach(v =>
            {
                v.particleMode = particleMode;
                v.voxelMode = voxelMode;
                v.level = level;
                v.size = size;
                v.edge = edge;
                v.useCapture = 1f - usePack;
                v.usePack = usePack;
                v.gridResolution = gridResolution;
                v.tileOffset = tileOffset;

                v.useEmission = useEmission;
                v.emissionRate = emissionRate;
                v.emissionRandom = emissionRandom;
                v.emissionOffset = emissionOffset;
            });

            if (Input.GetKeyDown(KeyCode.Keypad8)) Trigger(tileSpeedX, 0.5f);
            else if (Input.GetKeyDown(KeyCode.Keypad2)) Trigger(tileSpeedX, -0.5f);
            else if (Input.GetKeyDown(KeyCode.Keypad4)) Trigger(-0.5f, tileSpeedY);
            else if (Input.GetKeyDown(KeyCode.Keypad6)) Trigger(0.5f, tileSpeedY);
            else if(Input.GetKeyDown(KeyCode.Keypad0))
            {
                StopTrigger();
                tileSpeedY = 0.05f;
            }
            else if(Input.GetKeyDown(KeyCode.Keypad5))
            {
                StopTrigger();
                ResetTileOffset();
            }

            // if(Input.GetKeyDown(KeyCode.R)) Randomize();
        }

        protected void StopTrigger()
        {
            if(iTrigger != null)
            {
                callback();
                StopCoroutine(iTrigger);
            }
            iTrigger = null;
        }

        protected void Trigger(float sx, float sy)
        {
            StopTrigger();

            var tmpX = tileSpeedX;
            var tmpY = tileSpeedY;
            callback = () =>
            {
                tileSpeedX = tmpX;
                tileSpeedY = tmpY;
            };

            iTrigger = StartCoroutine(ITrigger(sx, sy, tileDuration, callback));
        }

        protected IEnumerator ITrigger(float sx, float sy, float duration, Action callback)
        {
            tileSpeedX = sx;
            tileSpeedY = sy;

            yield return new WaitForSeconds(duration);
            callback();

            iTrigger = null;
        }

        public void FlowRandom(float throttle = 0.1f)
        {
            voxels.ForEach(v =>
            {
                v.FlowRandom(throttle);
            });
        }

        public void Randomize()
        {
            voxels.ForEach(v =>
            {
                var bounds = v.bounds;
                var min = bounds.min;
                var max = bounds.max;
                var size = bounds.size;
                min.x = Random.Range(0f, 0.5f) * size.x + min.x;
                max.x = (max.x - min.x) * Random.Range(0.5f, 1f);
                min.y = Random.Range(0f, 0.5f) * size.y + min.y;
                max.y = (max.y - min.y) * Random.Range(0.5f, 1f);
                var nb = new Bounds();
                nb.SetMinMax(min, max);
                v.Voxelize(nb);
            });
        }

        public void ResetTileOffset()
        {
            tileOffset.Set(0f, 0f);
            tileSpeedX = tileSpeedY = 0f;
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            units.ForEach(u =>
            {
                Gizmos.DrawWireCube(u.bounds.center, u.bounds.size);
            });
        }

        public void PeakResolution()
        {
            if (!resolutionReactive) return;
            gridResolution = Random.Range(8, 80);
        }

        public void PeakTileOffset()
        {
            if (!tileReactive) return;

            var r = Random.value;
            if(r < 0.25f)
            {
                Trigger(tileSpeedX, 0.5f);
            } else if(r < 0.5f)
            {
                Trigger(tileSpeedX, -0.5f);
            } else if(r < 0.75f)
            {
                Trigger(-0.5f, tileSpeedY);
            } else
            {
                Trigger(0.5f, tileSpeedY);
            }
        }

        public override void Knob(int knobNumber, float knobValue)
        {
            switch(knobNumber)
            {
                case 17:
                    size = knobValue;
                    break;
                case 1:
                    gridResolution = (int)Mathf.Lerp(8, 80, knobValue);
                    break;
            }
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
            switch(note)
            {
                case 33:
                    usePack = Mathf.Clamp01(1f - usePack);
                    break;
                case 49:
                    useEmission = !useEmission;
                    break;
                case 65:
                    FlowRandom();
                    break;
            }
        }

        public override void OnOSC(string address, List<object> data)
        {
            switch(address)
            {
                case "/city_voxel/particle_mode":
                    particleMode = (ParticleMode)OSCUtils.GetIValue(data);
                    break;

                case "/city_voxel/flow":
                    FlowRandom(OSCUtils.GetFValue(data, 1, 0.1f));
                    break;

                case "/city_voxel/voxel_mode":
                    voxelMode = (CityVoxelMode)OSCUtils.GetIValue(data);
                    break;

                case "/city_voxel/level":
                    level = OSCUtils.GetIValue(data);
                    break;

                case "/city_voxel/size":
                    size = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/edge":
                    edge = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/capture":
                    useCapture = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/pack":
                    usePack = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/resolution":
                    gridResolution = OSCUtils.GetIValue(data);
                    break;

                case "/city_voxel/resolution/reactive":
                    resolutionReactive = OSCUtils.GetBoolFlag(data);
                    break;
                    
                case "/city_voxel/tile/reset":
                    ResetTileOffset();
                    break;

                case "/city_voxel/tile/speed/x":
                    tileSpeedX = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/tile/speed/y":
                    tileSpeedY = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/tile/reactive":
                    tileReactive = OSCUtils.GetBoolFlag(data);
                    break;

                case "/city_voxel/emission/use":
                    useEmission = OSCUtils.GetBoolFlag(data);
                    break;

                case "/city_voxel/emission/rate":
                    emissionRate = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/emission/speed":
                    emissionSpeed = OSCUtils.GetFValue(data);
                    break;

                case "/city_voxel/emission/random":
                    emissionRandom = OSCUtils.GetFValue(data);
                    break;

            }
        }

    }

}


