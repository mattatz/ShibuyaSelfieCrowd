Shader "VJ/PostEffects/Inverse"
{

  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _T ("T", Range(0, 1)) = 1.0
    _Scale ("Scale", Float) = 35.0
    _Power ("Power", Range(0.1, 2.0)) = 0.75

    _UseDepth ("Use Depth", Range(0, 1)) = 0.0
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
      #include "Assets/Common/Shaders/Noise/SimplexNoise2D.hlsl"
      #include "Assets/Common/Shaders/Random.hlsl"

      UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

      float fbm(float2 P, float lacunarity, float gain)
      {
        float sum = 0.0;
        float amp = 1.0;
        float2 pp = P;
        for (int i = 0; i < 4; i += 1)
        {
          amp *= gain;
          sum += amp * (snoise(pp) + 1.0) * 0.5;
          pp *= lacunarity;
        }
        return sum;
      }

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
      float _T;
      float _Scale, _Power;

      fixed _UseDepth;

      fixed4 frag(v2f IN) : SV_Target
      {
        fixed4 col = tex2D(_MainTex, IN.uv);

        float s = nrand(float2(IN.uv.y, _Time.x));
        float n = (snoise(float2(_Time.y + step(s, IN.uv.x) * _Scale, IN.uv.y * _Scale)) + 1.0) * 0.5;
        float nt = step(pow(n, _Power), _T);
        fixed3 inverse_noise = lerp(col.rgb, 1.0 - col.rgb, nt);

        half depth = saturate(Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, IN.uv))));
        fixed3 inverse_depth = lerp(col.rgb, 1.0 - col.rgb, step(depth, _T));

        col.rgb = lerp(inverse_noise, inverse_depth, _UseDepth);

        return col;
      }

      ENDCG

    }

  }
}
