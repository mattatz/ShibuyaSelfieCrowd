Shader "VJ/CityVoxelParticleSystem"
{
  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    [HDR] _Emission ("Emission", Color) = (0, 0, 0, 0)
    _MainTex ("Albedo", 2D) = "white" {}

    _CaptureTex ("Capture", 2D) = "white" {}
    _CaptureBoundsMin ("Capture Bounds Min", Vector) = (0, 0, 0, -1)
    _CaptureBoundsSize ("Capture Bounds Size", Vector) = (1, 1, 1, -1)
    _UseCapture ("Use Capture", Range(0.0, 1.0)) = 0

    _Pack ("Pack", 2D) = "white" {}
    _PackSize ("Pack", Float) = 0.25 // 1 / count
    _UsePack ("Use Pack", Range(0.0, 1.0)) = 0

    [Space]
    _Glossiness ("Smoothness", Range(0, 1)) = 0.5
    [Gamma] _Metallic("Metallic", Range(0, 1)) = 0

    [Space]
    _Size ("Size", Float) = 0.1
    _Edge ("Edge", Range(0.0, 0.5)) = 0.5

    [KeywordEnum(None, Front, Back)] _Cull ("Cull", Int) = 2
  }

  SubShader
  {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

    Pass
    {
      Cull [_Cull]
      Tags { "LightMode"="Deferred" }
      CGPROGRAM
      #pragma target 4.0
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
      #include "CityVoxelParticleSystem.hlsl"
      ENDCG
    }

    Pass
    {
      Cull [_Cull]
      Tags { "LightMode"="ShadowCaster" }
      CGPROGRAM
      #pragma target 4.0
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma multi_compile_shadowcaster noshadowmask nodynlightmap nodirlightmap nolightmap
      #include "CityVoxelParticleSystem.hlsl"
      ENDCG
    }
  }
}
