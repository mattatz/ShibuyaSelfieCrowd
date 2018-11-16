Shader "VertexAnim/VisNormal" {
  Properties{
    _MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
    _Color("Color", Color) = (1,1,1,1)

    _AnimTex("PosTex", 2D) = "white" {}
    _AnimTex_NormalTex("Normal Tex", 2D) = "white" {}
    _AnimTex_Scale("Scale", Vector) = (1,1,1,1)
    _AnimTex_Offset("Offset", Vector) = (0,0,0,0)
    _AnimTex_AnimEnd("End (Time, Frame)", Vector) = (0, 0, 0, 0)
    _AnimTex_T("Time", float) = 0
    _AnimTex_FPS("Frame per Sec(FPS)", Float) = 30
  }

  SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 700 Cull Back

    Pass {
      CGPROGRAM
      #pragma multi_compile BILINEAR_OFF BILINEAR_ON
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      #include "AnimTexture.cginc"

      struct vsin {
        uint vid: SV_VertexID;
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
        float3 normal : NORMAL;
      };

      struct vs2ps {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float3 normal : NORMAL;
      };

      sampler2D _MainTex;
      float4 _Color;

      vs2ps vert(vsin v) {
        float t = _AnimTex_T + _Time.y;
        t = clamp(fmod(t, _AnimTex_AnimEnd.x), 0, _AnimTex_AnimEnd.x);
        v.vertex.xyz = AnimTexVertexPos(v.vid, t);
        float3 n = AnimTexNormal(v.vid, t);
        // n = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, n));

        vs2ps OUT;
        OUT.vertex = UnityObjectToClipPos(float4(v.vertex.xyz, 1));
        OUT.uv = v.texcoord;
        OUT.normal = UnityObjectToWorldNormal(v.normal);
        return OUT;
      }

      float4 frag(vs2ps IN) : COLOR{
        return float4(0.5 * (IN.normal + 1.0), 1.0);
      }

      ENDCG
    }

    Pass {
      Name "ShadowCaster"
      Tags { "LightMode" = "ShadowCaster" }

      CGPROGRAM
      #pragma target 5.0
      #pragma multi_compile BILINEAR_OFF BILINEAR_ON
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      #include "AnimTexture.cginc"

      struct vsin {
        uint vid: SV_VertexID;
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 texcoord : TEXCOORD0;
      };

      struct vs2ps {
        V2F_SHADOW_CASTER;
      };

      sampler2D _MainTex;
      float4 _Color;

      vs2ps vert(vsin v) {
        float t = _AnimTex_T;
        t = clamp(t, 0, _AnimTex_AnimEnd.x);
        v.vertex.xyz = AnimTexVertexPos(v.vid, t);

        vs2ps OUT;
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT);
        return OUT;
      }

      float4 frag(vs2ps IN) : COLOR {
        SHADOW_CASTER_FRAGMENT(IN);
      }
      ENDCG
    }
  }

  FallBack Off
}
