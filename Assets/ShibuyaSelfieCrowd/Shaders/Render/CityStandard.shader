Shader "VJ/City"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
    _UseTexture ("Use Texture", Range(0.0, 1.0)) = 1.0

		[HDR] _Emission ("Emission", Color) = (0, 0, 0, 0)

    [Space] _Glossiness ("Smoothness", Range(0, 1)) = 0.5
    [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0

    _Center ("Center", Range(0.0, 1.0)) = 0.0

    [Toggle] _Wireframe ("Wireframe", Float) = 0
    _Gain ("Gain", Range(0, 10)) = 1.0
    _Threshold ("Threshold", Range(0, 1)) = 0.5

    _NoiseScale ("Noise Scale", Float) = 0.5
    _NoiseIntensity ("Noise Intensity", Float) = 2.5
    _NoiseSpeed ("Noise Speed", Float) = 1.0

    [KeywordEnum(None, Front, Back)] _Cull ("Cull", Int) = 2
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

		Pass
		{
      Cull [_Cull]
      Tags { "LightMode" = "Deferred" }

			CGPROGRAM
      #pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
      #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
			#include "./CityStandard.hlsl"
			ENDCG
		}

		Pass
		{
      Cull [_Cull]
      Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
      #pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
      #pragma multi_compile_shadowcaster noshadowmask nodylightmap nodirlightmap nolightmap
			#include "./CityStandard.hlsl"
			ENDCG
		}

		Pass
		{
      Cull [_Cull]
      Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

      struct appdata {
        float4 position : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float4 position : SV_Position;
        float2 uv : TEXCOORD0;
      };

      v2f vert(appdata IN) {
        v2f OUT;
        OUT.position = UnityObjectToClipPos(IN.position);
        OUT.uv = IN.uv;
        return OUT;
      }

      sampler2D _MainTex;

      fixed4 frag(v2f IN) : COLOR {
        return tex2D(_MainTex, IN.uv);
      }

			ENDCG
		}

	}
}
