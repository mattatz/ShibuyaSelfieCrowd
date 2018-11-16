using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ
{

    public class Instagram : MonoBehaviour {

        [SerializeField] protected List<Texture2D> images;
        [SerializeField, Range(512, 4096)] protected int resolution = 2048;
        [SerializeField] protected Material material;

        public Texture2D Pack()
        {
            var rt = Generate();
            var tex = new Texture2D(rt.width, rt.height);

            var tmp = RenderTexture.active;
            {
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
            }
            RenderTexture.active = tmp;

            return tex;
        }

        protected RenderTexture Generate()
        {
            var buffer = CreateTexture(resolution, resolution);

            int sq = Mathf.FloorToInt(Mathf.Sqrt(images.Count));
            float invSq = 1f / sq;

            for(int y = 0; y < sq; y++)
            {
                var offset = y * sq;
                for(int x = 0; x < sq; x++)
                {
                    var img = images[offset + x];
                    Map(img, buffer, x * invSq, y * invSq, invSq, invSq);
                }
            }

            return buffer;
        }

        protected void OnDestroy()
        {
        }

        int Pow2(int n)
        {
            if (n < 0)
                return 0;

            if (n == 1)
                return 1;

            return (int)Mathf.Pow(2f, Mathf.Floor(Mathf.Log(n - 1, 2f)));
        }

        protected void Map(Texture2D src, RenderTexture dst, float x, float y, float w, float h)
        {
            var tmp = RenderTexture.active;
            {
                RenderTexture.active = dst;

                GL.PushMatrix();
                GL.LoadOrtho();

                material.SetPass(0);
                material.SetTexture("_MainTex", src);

                DrawQuad(x, y, w, h);

                GL.PopMatrix();
            }
            RenderTexture.active = tmp;
        }

        protected void DrawQuad(float x, float y, float w, float h)
        {
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0f, 0f);
            GL.Vertex3(x, y, 0f);

            GL.TexCoord2(1f, 0f);
            GL.Vertex3(x + w, y, 0f);

            GL.TexCoord2(1f, 1f);
            GL.Vertex3(x + w, y + h, 0f);

            GL.TexCoord2(0f, 1f);
            GL.Vertex3(x, y + h, 0f);

            GL.End();
        }

        protected RenderTexture CreateTexture(int w, int h)
        {
            var rt = new RenderTexture(w, h, 0);
            rt.format = RenderTextureFormat.ARGBHalf;
            rt.filterMode = FilterMode.Bilinear;
            rt.Create();
            return rt;
        }

    }

}


