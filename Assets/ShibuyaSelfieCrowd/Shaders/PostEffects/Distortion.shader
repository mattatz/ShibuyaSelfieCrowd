Shader "VJ/PostEffects/Distortion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Scale ("Scale", Range(0, 1)) = 0.25
    _Offset ("Offset", Float) = 0
    _Border ("Border", Range(0, 1.0)) = 0.05
    _T ("T", Range(0, 1)) = 1.0
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
			#include "Assets/Common/Shaders/Noise/SimplexNoise3D.hlsl"

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
      fixed _Scale, _T;
      float _Offset;

      fixed _Border;

      fixed4 frag(v2f i) : SV_Target
      {
        float r = _ScreenParams.x / _ScreenParams.y;
        float2 uv = i.uv;
        float x = uv.x * r, y = uv.y;
        float2 displacement = snoise(float3(float2(x, y) * _Scale, _Offset)).xy;
        float s = smoothstep(0, _Border * r, uv.x) * smoothstep(1.0, 1.0 - _Border * r, uv.x) * smoothstep(0, _Border, uv.y) * smoothstep(1.0, 1.0 - _Border, uv.y);
        uv.xy += displacement * s * _T;

        float2 t = frac(uv * 0.5) * 2.0;
        float2 length = float2(1.0, 1.0);
        uv = length - abs(t - length);
        fixed4 col = tex2D(_MainTex, uv);
        return col;
      }

			ENDCG
		}
	}
}
