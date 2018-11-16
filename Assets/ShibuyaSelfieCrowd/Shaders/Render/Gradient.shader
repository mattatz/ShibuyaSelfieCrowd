Shader "VJ/Gradient"
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
        float l = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0)).x;
        float r = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).x;
        float t = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y)).x;
        float b = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).x;
        float dx = l - r;
        float dy = t - b;
        return half4(abs(dx), abs(dy), 0, 1) * 10;
			}

			ENDCG
		}
	}
}
