Shader "VJ/Depth"
{

	Properties
	{
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float4 pos : SV_POSITION;
        float2 depth : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
        o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
        UNITY_TRANSFER_DEPTH(o.depth);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
        // UNITY_OUTPUT_DEPTH(i.depth);
        return (i.pos.z / i.pos.w).xxxx;
			}

			ENDCG
		}
	}
}
