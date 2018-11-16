Shader "VJ/Mask"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Threshold ("Threshold", Range(0, 1)) = 0.25
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

      float _Threshold;
			
			half4 frag (v2f i) : SV_Target
			{
        float depth = tex2D(_MainTex, i.uv).x;
        clip(depth - _Threshold);
        return half4(1, 1, 1, 1);
			}

			ENDCG
		}
	}
}
