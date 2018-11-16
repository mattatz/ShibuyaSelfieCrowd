Shader "VJ/PostEffects/Mirror"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    [Toggle] _Horizontal ("Horizontal", Float) = 0
    [Toggle] _Left ("Left", Float) = 0
    [Toggle] _Vertical ("Vertical", Float) = 0
    [Toggle] _Up ("Up", Float) = 0
  }

  SubShader
  {
    Cull Off ZWrite Off ZTest Always

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      sampler2D _MainTex;
      fixed _Horizontal, _Vertical;
      fixed _Left, _Up;

      fixed4 frag(v2f i) : SV_Target
      {
        float2 uv = i.uv;
        float x = uv.x, ix = 1.0 - uv.x;
        float sx = step(0.5, x);
        float y = uv.y, iy = 1.0 - uv.y;
        float sy = step(0.5, y);
        uv.x = lerp(lerp(x, ix, sx * _Horizontal), lerp(x, ix, (1.0 - sx) * _Horizontal), _Left);
        uv.y = lerp(lerp(y, iy, sy * _Vertical), lerp(y, iy, (1.0 - sy) * _Vertical), _Up);
        return tex2D(_MainTex, uv);
      }

      ENDCG
    }

  }
}
