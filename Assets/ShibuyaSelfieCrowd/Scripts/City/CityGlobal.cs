using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VJ
{

    public class CityGlobal : SingletonMonoBehaviour<CityGlobal> {

        #region Accessors

        public RenderTexture OrthographicColor { get; set; }
        public RenderTexture OrthographicDepth { get; set; }
        public RenderTexture OrthographicSDF { get; set; }
        public Bounds WholeBounds { get; set; }

        public Dictionary<TileTag, CityUnit> Tiles { get; set; }

        #endregion

        protected void OnEnable()
        {
            Tiles = new Dictionary<TileTag, CityUnit>();
            GetComponentsInChildren<CityUnit>().ToList().ForEach(u =>
            {
                Tiles.Add(u.Tile, u);
            });
        }

        protected void Start()
        {
        }

        public void OnCaptureBounds(Bounds bounds)
        {
            WholeBounds = bounds;
        }

        public void OnCaptureOrthographicColor(RenderTexture color)
        {
            OrthographicColor = color;
        }

        public void OnCaptureOrthographicDepth(RenderTexture depth)
        {
            OrthographicDepth = depth;
        }

        public void OnCaptureOrthographicSDF(RenderTexture sdf)
        {
            OrthographicSDF = sdf;
        }

    }

}


