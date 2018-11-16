Shader "VJ/Map"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
      // Blend One One

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
				float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
        o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
				return o;
			}

      sampler2D _MainTex;
      float4 _MainTex_TexelSize;

			half4 frag (v2f i) : SV_Target
			{
        return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}
