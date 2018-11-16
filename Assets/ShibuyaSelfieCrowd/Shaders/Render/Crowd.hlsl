#ifndef __CROWD_COMMON_INCLUDED__
#define __CROWD_COMMON_INCLUDED__

#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"

#include "Assets/Common/Shaders/Random.hlsl"
#include "Assets/Common/Shaders/Noise/SimplexNoise3D.hlsl"
#include "Assets/Common/Shaders/Quaternion.hlsl"
#include "Assets/Packages/VertexAnimator/Shader/AnimTexture.cginc"

#include "../Common/Human.hlsl"
#include "../Common/VertexAnimAttribute.hlsl"
#include "../Common/Emission.hlsl"

#define IDLE 0
#define WALK 1

StructuredBuffer<Human> _Crowd;
StructuredBuffer<VertexAnimAttribute> _Attributes;

half4 _Color, _Emission;

half _Glossiness;
half _Metallic;

float _NoiseScale, _NoiseIntensity, _NoiseOffset;

sampler2D _Faces;
fixed _UseFace;
float _Scale;
float2 _Offset;
float _FaceRandom;
float2 _Scroll;

sampler2D _PosTex_Idle, _NormTex_Idle;
sampler2D _PosTex_Walk, _NormTex_Walk;

void animate(
  uint idx, sampler2D posTex, sampler2D normTex, in uint vid, in float t,
  inout float3 pos, inout float3 norm
)
{
  VertexAnimAttribute attr = _Attributes[idx];
  float tt = clamp(t % attr.end.x, 0, attr.end.x);
  pos = AnimTexVertexPos(posTex, attr.texel, vid, tt, attr.fps, attr.end, attr.scale, attr.offset);
  norm = normalize(AnimTexNormal(normTex, attr.texel, vid, tt, attr.fps, attr.end));
}

void sample_idle(in uint vid, in float t, inout float3 pos, inout float3 norm)
{
  animate(IDLE, _PosTex_Idle, _NormTex_Idle, vid, t, pos, norm);
}

void sample_walk(in uint vid, in float t, inout float3 pos, inout float3 norm)
{
  animate(WALK, _PosTex_Walk, _NormTex_Walk, vid, t, pos, norm);
}

struct appdata
{
  float4 vertex : POSITION;
  float2 uv : TEXCOORD0;
  float3 normal : NORMAL;
  uint vid : SV_VertexID;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
  float4 position : SV_POSITION;
#if defined(PASS_CUBE_SHADOWCASTER)
  // Cube map shadow caster pass
  float3 shadow : TEXCOORD0;
#elif defined(UNITY_PASS_SHADOWCASTER)
  // Default shadow caster pass
#else
  float3 normal : NORMAL;
  float3 wnrm : TANGENT;
  half3 ambient : TEXCOORD0;
  float3 wpos : TEXCOORD1;
  float2 uv : TEXCOORD2;
  float2 uv2 : TEXCOORD3;
  half4 emission : COLOR;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

void animate(inout appdata IN, in Human h, uint iid : SV_InstanceID)
{
  float t = _Time.y;
  float offset = nrand(float2(iid, 0)) * 100.0;
  t += offset;

  float3 idle_p, idle_n;
  float3 walk_p, walk_n;

  sample_idle(IN.vid, t * 0.25, idle_p, idle_n);
  sample_walk(IN.vid, t, walk_p, walk_n);

  static const float thres_walk = 0.3;
  static const float inv_thres_walk = 1.0 / thres_walk;
  float speed = saturate(length(h.velocity.xz));

  // weight for animation blending
  float w = saturate(speed * inv_thres_walk);
  IN.vertex.xyz = lerp(idle_p, walk_p, w);
  IN.normal.xyz = lerp(idle_n, walk_n, w);
}

v2f vert (appdata IN, uint iid : SV_InstanceID)
{
  v2f OUT;

  Human h = _Crowd[iid];

  float3 lvtx = IN.vertex.xyz;
  float3 lnrm = IN.normal.xyz;

  if (h.alive)
  {
    animate(IN, h, iid);
  }

  float4 r = h.rotation;
  IN.vertex.xyz = rotate_vector(IN.vertex.xyz, r) * h.scale.xyz * h.lifetime + h.position.xyz;
  IN.normal.xyz = rotate_vector(IN.normal.xyz, r);

  float4 vertex = float4(IN.vertex.xyz, 1);
  float3 wpos = mul(unity_ObjectToWorld, vertex).xyz;

  float3 normal = IN.normal.xyz;
  float3 wnrm = UnityObjectToWorldNormal(normal);

#if defined(PASS_CUBE_SHADOWCASTER)
  // Cube map shadow caster pass: Transfer the shadow vector.
  OUT.position = UnityObjectToClipPos(float4(wpos.xyz, 1));
  OUT.shadow = wpos.xyz - _LightPositionRange.xyz;
#elif defined(UNITY_PASS_SHADOWCASTER)
  // Default shadow caster pass: Apply the shadow bias.
  float scos = dot(wnrm, normalize(UnityWorldSpaceLightDir(wpos.xyz)));
  wpos.xyz -= wnrm * unity_LightShadowBias.z * sqrt(1 - scos * scos);
  OUT.position = UnityApplyLinearShadowBias(UnityWorldToClipPos(float4(wpos.xyz, 1)));
#else
  OUT.position = UnityWorldToClipPos(float4(wpos.xyz, 1));
  OUT.normal = lnrm;
  OUT.wnrm = wnrm;

  OUT.ambient = ShadeSHPerVertex(wnrm, 0);
  OUT.wpos = wpos.xyz;

  OUT.uv = IN.uv;
  OUT.uv = (lvtx.xy * _Scale) + _Offset;

  static const float count = 44;
  static const float inv = 1.0 / count;

  float x = nrand(float2(13.7, iid));
  float2 scroll = _Scroll.xy + x * _FaceRandom;
  OUT.uv2 = OUT.uv * inv + floor(count * float2(nrand(float2(iid, floor(scroll.x))), nrand(float2(floor(scroll.y), iid)))) * inv;
  OUT.emission = emission(iid, float4(0, 0, 0, 0), _Emission);

#endif
  return OUT;
}

#if defined(PASS_CUBE_SHADOWCASTER)

half4 frag(v2f IN) : SV_Target
{
  float depth = length(IN.shadow) + unity_LightShadowBias.x;
  return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

half4 frag(v2f IN) : SV_Target 
{
  return 0; 
}

#else

void frag(v2f IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3) 
{
  float outside = step(dot(IN.normal, float3(0, 0, 1)), 0) + step(IN.uv.x, 0) + step(1, IN.uv.x) + step(IN.uv.y, 0) + step(1, IN.uv.y);
  outside = saturate(outside);
  half3 albedo = lerp(tex2D(_Faces, IN.uv2).rgb, _Color.rgb, outside);
  albedo = lerp(_Color.rgb, albedo, _UseFace);

  half3 c_spec;
  half refl10;

  half3 c_diff = DiffuseAndSpecularFromMetallic(
    albedo, _Metallic, // input
    c_spec, refl10 // output
  );

  UnityStandardData data;
  data.diffuseColor = c_diff;
  data.occlusion = 1.0;
  data.specularColor = c_spec;
  data.smoothness = _Glossiness;
  data.normalWorld = normalize(IN.wnrm);
  UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

  half3 sh = ShadeSHPerPixel(data.normalWorld, IN.ambient, IN.wpos);
  float4 emission = IN.emission * float4(albedo, 1);
  outEmission = emission * saturate((1.0 - outside) * _UseFace) + half4(sh * c_diff, 1);
}

#endif

#endif
