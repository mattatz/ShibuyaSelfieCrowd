Shader "Unlit/Debug"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Scale ("Scale", Range(0.0, 1.0)) = 0.5
    _Offset ("Offset", Vector) = (0, 0, -1, -1)
    _Scroll ("Scroll", Vector) = (0, 0, -1, -1)
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
        float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
        float2 uv2 : TEXCOORD1;
        float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

      float _Scale;
      float2 _Offset;
      float2 _Scroll;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = (v.vertex.xy * _Scale) + _Offset;

        static const float inv = 0.0625;
        o.uv2 = o.uv * inv + floor(_Scroll.xy) * inv;
        o.uv2 = fmod(o.uv2, float2(1, 1));

        o.normal = v.normal;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        float2 uv = i.uv;
        float flag = step(uv.x, 0) + step(1, uv.x) + step(uv.y, 0) + step(1, uv.y);
        flag += step(dot(i.normal, float3(0, 0, 1)), 0);

				fixed4 col = tex2D(_MainTex, i.uv2);
        col.rgb = lerp(col, fixed3(1, 1, 1), saturate(flag));
				return col;
			}
			ENDCG
		}
	}
}
