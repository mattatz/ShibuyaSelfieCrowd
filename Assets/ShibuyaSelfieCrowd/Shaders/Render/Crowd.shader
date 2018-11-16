Shader "VJ/Crowd" {

  Properties {
    _Color ("Color", Color) = (1, 1, 1, 1)
		[HDR] _Emission ("Emission", Color) = (1, 1, 1, 1)

    _Glossiness ("Smoothness", Range(0, 1)) = 0.5
    _Metallic ("Metallic", Range(0, 1)) = 0.0

    _Faces ("Faces", 2D) = "" {}
    _UseFace ("Use face", Range(0.0, 1.0)) = 0.0
    _Scale ("Scale", Range(0.0, 1.0)) = 0.5
    _Offset ("Offset", Vector) = (0, 0, -1, -1)
    _Scroll ("Scroll", Vector) = (0, 0, -1, -1)

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
			#include "./Crowd.hlsl"
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
			#include "./Crowd.hlsl"
			ENDCG
		}

	}

}