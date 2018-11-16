#ifndef __CITY_COMMON_INCLUDED__
#define __CITY_COMMON_INCLUDED__

#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"

#include "Assets/Common/Shaders/Noise/SimplexNoise3D.hlsl"
#include "../Common/CityAttribute.hlsl"
#include "../Common/Dithering.hlsl"

half4 _Color, _Emission;
sampler2D _MainTex;
float4 _MainTex_ST;
fixed _UseTexture;

half _Glossiness;
half _Metallic;

fixed _Center;
half _Wireframe, _Gain, _Threshold;

float3 _CaptureBoundsMin, _CaptureBoundsMax;
sampler2D _CaptureDepth;

StructuredBuffer<CityAttribute> _Attributes;

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
  float3 bary : TANGENT;
  float4 screen : TEXCOORD0;
#if defined(PASS_CUBE_SHADOWCASTER)
  float3 shadow : TEXCOORD1;
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  float3 normal : NORMAL;
  float2 uv : TEXCOORD1;
  half3 ambient : TEXCOORD2;
  float3 wpos : TEXCOORD3;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

float2 ground_uv(float3 p)
{
  float3 size = _CaptureBoundsMax.xyz - _CaptureBoundsMin.xyz;
  float ux = (p.x - _CaptureBoundsMin.x) / size.x;
  float uz = (p.z - _CaptureBoundsMin.z) / size.z;
  return float2(ux, uz);
}

float3 ground(float3 p)
{
  float3 size = _CaptureBoundsMax.xyz - _CaptureBoundsMin.xyz;
  float2 uv = ground_uv(p);
  float depth = tex2Dlod(_CaptureDepth, float4(uv, 0, 0)).x;
  p.y = depth * size.y + _CaptureBoundsMin.y;
  return p;
}

float wireframe(v2f IN)
{
  float3 d = fwidth(IN.bary);
  float3 a3 = smoothstep((0.0).xxx, d * _Gain, IN.bary);
  float t = min(min(a3.x, a3.y), a3.z);
  return t;
}

void wireframe_clip(v2f IN)
{
  float t = wireframe(IN);
  float tt = lerp(1.0, 1.0 - t, _Wireframe);
  dither_clip(IN.screen.xy, tt);
}

float _NoiseScale, _NoiseIntensity, _NoiseOffset;

float3 wobble(float3 p, float3 norm)
{
  float3 seed = p * _NoiseScale;
  float3 n = snoise(seed + float3(0, 0, _NoiseOffset));
  // p += norm * (n + 1.0) * 0.5 * _NoiseIntensity;
  return (n + 1.0) * 0.5 * _NoiseIntensity;
}

v2f vert (appdata IN, uint iid : SV_InstanceID)
{
  v2f OUT;

  CityAttribute attr = _Attributes[IN.vid];

  float3 position = IN.vertex.xyz;
  position = lerp(position, attr.center, _Center);
  position += wobble(position, IN.normal.xyz);

  OUT.bary = attr.wireframe;

  float4 vertex = float4(position, 1);
  float3 wpos = mul(unity_ObjectToWorld, vertex).xyz;

  // wpos = ground(wpos);

  float3 normal = IN.normal;
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
  OUT.normal = wnrm;
  OUT.uv = IN.uv;
  OUT.ambient = ShadeSHPerVertex(wnrm, 0);
  OUT.wpos = wpos.xyz;
#endif
  OUT.screen = ComputeScreenPos(OUT.position);
  return OUT;
}

#if defined(PASS_CUBE_SHADOWCASTER)

half4 frag(v2f IN) : SV_Target
{
  wireframe_clip(IN);
  float depth = length(IN.shadow) + unity_LightShadowBias.x;
  return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

half4 frag(v2f IN) : SV_Target 
{
  wireframe_clip(IN);
  return 0; 
}

#else

void frag(v2f IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3) 
{
  wireframe_clip(IN);

  half4 col = tex2D(_MainTex, IN.uv);
  half3 albedo = lerp(_Color.rgb, col.rgb, _UseTexture);

  half3 c_diff, c_spec;
  half refl10;
  c_diff = DiffuseAndSpecularFromMetallic(
    albedo, _Metallic,
    c_spec, refl10
  );

  UnityStandardData data;
  data.diffuseColor = c_diff;
  data.occlusion = 1.0;
  data.specularColor = c_spec;
  data.smoothness = _Glossiness;
  data.normalWorld = normalize(IN.normal);
  UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

  half3 sh = ShadeSHPerPixel(data.normalWorld, IN.ambient, IN.wpos);
  outEmission = half4(albedo, 1) * _Emission + half4(sh * c_diff, 1);
}

#endif

#endif
