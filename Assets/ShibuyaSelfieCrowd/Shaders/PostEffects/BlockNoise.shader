// This is port of below code.
// [Shadertoy] EM Interference Effect
// https://www.shadertoy.com/view/lsXSWl

Shader "VJ/PostEffects/BlockNoise" {

	Properties {
		_MainTex ("-", 2D) = "" {}
		_T ("T", Range(0, 1)) = 1.0

		_Speed ("Speed", Range(0, 10)) = 1.0
		_Shift ("Shift", Range(0, 0.25)) = 0.05
		_ScaleS ("Scale S", Vector) = (24, 9, -1, -1)
		_ScaleL ("Scale L", Vector) = (8, 4, -1, -1)
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "Assets/Common/Shaders/Noise/SimplexNoise3D.hlsl"
	
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	float _Speed, _Shift;
	float2 _ScaleS, _ScaleL;

	float _T;
	
	float rng2(float2 seed)
	{
	    return frac(sin(dot(seed * floor(_Time.y * _Speed), fixed2(127.1, 311.7))) * 43758.5453123);
	} 

	float rng(float seed)
	{
	    return rng2 (fixed2 (seed, 1.0));
	}

	float4 em_interference(v2f_img i) {
		float2 uv = i.uv.xy;
	    float2 blockS = floor(uv * _ScaleS);
	    float2 blockL = floor(uv * _ScaleL);
	    
	    float lineNoise = pow(rng2(blockS), 8.0) * pow(rng2(blockL), 3.0) - pow(rng(7.2341), 17.0) * 2.0;
	    
	    fixed4 col1 = tex2D(_MainTex, uv);
	    fixed4 col2 = tex2D(_MainTex, uv + float2(lineNoise * _Shift * rng( 5.0), 0));
	    fixed4 col3 = tex2D(_MainTex, uv - float2(lineNoise * _Shift * rng(31.0), 0));
	    
		return lerp(tex2D(_MainTex, i.uv), fixed4(fixed3(col1.x, col2.y, col3.z), 1.0), _T);
	}

	float4 frag (v2f_img i) : SV_Target
	{	
		return em_interference(i);
	}
	
	ENDCG
	
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment frag
			ENDCG
		} 
	}
}