Shader "VJ/PostEffects/Repeat"
{

  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    [Toggle] _Horizontal ("Horizontal", Float) = 0
    _Count ("Count", Float) = 4
    _Unit ("Unit", Float) = 0.25
    _Offset ("Offset", Float) = 0

    [Toggle] _Randomize ("Randomize", Float) = 0
    _RandomSeed ("Random Seed", Float) = 0
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

      #include "Assets/Common/Shaders/Random.hlsl"

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

      float _Count, _Unit, _Offset;
      half _RandomSeed;
      fixed _Horizontal, _Randomize;

      fixed4 frag(v2f i) : SV_Target
      {
        float2 uv = i.uv;
        float ix = floor(uv.x * _Count);
        float iy = floor(uv.y * _Count);

        float off = (1.0 - _Unit) * 0.5;
        float rx = lerp(-off, off, nrand(float2(ix, _RandomSeed)));
        float ry = lerp(-off, off, nrand(float2(_RandomSeed, iy)));
        uv.x = lerp(uv.x, off + fmod(uv.x, _Unit) + lerp(_Offset, rx, _Randomize), _Horizontal);
        uv.y = lerp(uv.y, off + fmod(uv.y, _Unit) + lerp(_Offset, ry, _Randomize), 1.0 - _Horizontal);
        // uv.x = fmod(uv.x, 1.0);
        // uv.y = fmod(uv.y, 1.0);
        return tex2D(_MainTex, uv);
      }

      ENDCG
    }

  }
}
