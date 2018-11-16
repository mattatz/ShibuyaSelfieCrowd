
#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"

#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
#define PASS_CUBE_SHADOWCASTER
#endif

#include "Assets/Common/Shaders/Random.hlsl"
#include "Assets/Common/Shaders/Quaternion.hlsl"

#include "../Common/CityVoxelParticle.hlsl"
#include "./VoxelEmission.hlsl"

StructuredBuffer<VParticle> _ParticleBuffer;

half4 _Color;

sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _CaptureTex;
float3 _CaptureBoundsMin, _CaptureBoundsSize;
float2 _TileOffset;
fixed _UseCapture;

sampler2D _Pack;
fixed _UsePack;

float4 _Pack_TexelSize;
float _PackSize;

half _Glossiness;
half _Metallic;

float _Size, _Edge;

float4 _EmissionBase;
sampler2D _Gradient;

struct appdata
{
  float4 position : POSITION;
  uint vid : SV_VertexID;
};

struct Attributes
{
  float4 position : POSITION;
  float4 rotation : TANGENT;
  float3 size : NORMAL;
  float3 color : COLOR;
  float2 uv : TEXCOORD0;
  float4 emission : TEXCOORD1;
  float3 origin : TEXCOORD2;
  uint vid : TEXCOORD3;
};

struct Varyings
{
  float4 position : SV_POSITION;
  float2 uv : TEXCOORD0;
#if defined(PASS_CUBE_SHADOWCASTER)
  float3 shadow : TEXCOORD1;
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  half3 ambient : TEXCOORD1;
  float3 wpos : TEXCOORD2;
  float4 emission : TEXCOORD3;
  float2 uv2 : TEXCOORD4;
  float3 origin : TEXCOORD5;
  float2 wuv : TEXCOORD6;
  float3 color : COLOR;
  float3 normal : NORMAL;
#endif
};

Attributes vert(appdata input)
{
  Attributes OUT;

  uint vid = input.vid;

  VParticle particle = _ParticleBuffer[vid];
  OUT.position = float4(particle.position, 1);
  OUT.size = particle.scale * _Size;
  OUT.rotation = particle.rotation;
  OUT.color = tex2Dlod(_MainTex, float4(particle.uv, 0, 0)).rgb;
  OUT.vid = vid;
#if defined(PASS_CUBE_SHADOWCASTER)
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  float x = nrand(float2(vid, 0));
  float y = nrand(float2(0, vid));
  OUT.uv = float2((floor(x / _PackSize) + 0.5) * _PackSize, (floor(y / _PackSize) + 0.5) * _PackSize);
  OUT.emission = emission(vid, (0.0).xxxx, _EmissionBase);
  OUT.origin = particle.origin;
#endif
  return OUT;
}

Varyings vert_out(in Attributes IN, in Varyings o, float4 pos, float3 wnrm)
{
  float3 wpos = mul(unity_ObjectToWorld, pos).xyz;

#if defined(PASS_CUBE_SHADOWCASTER)
  o.position = UnityObjectToClipPos(float4(wpos.xyz, 1));
  o.shadow = wpos.xyz - _LightPositionRange.xyz;
#elif defined(UNITY_PASS_SHADOWCASTER)
  float scos = dot(wnrm, normalize(UnityWorldSpaceLightDir(wpos.xyz)));
  wpos.xyz -= wnrm * unity_LightShadowBias.z * sqrt(1 - scos * scos);
  o.position = UnityApplyLinearShadowBias(UnityWorldToClipPos(float4(wpos.xyz, 1)));
#else
  o.position = UnityWorldToClipPos(float4(wpos.xyz, 1));
  o.normal = wnrm;
  o.ambient = ShadeSHPerVertex(wnrm, 0);
  o.wpos = wpos.xyz;
  o.uv2 = IN.uv;
  o.color = IN.color;
  o.emission = IN.emission;
#endif

  return o;
}

void add_face(in Attributes IN, inout TriangleStream<Varyings> OUT, float4 p[4], float3 normal, float2 wuv[4])
{
  float3 wnrm = UnityObjectToWorldNormal(normal);
  Varyings o;
  UNITY_INITIALIZE_OUTPUT(Varyings, o);

  static const float2 ouv[4] = { 
    float2(0, 0),
    float2(0, 1),
    float2(1, 0),
    float2(1, 1)
  };

  float2 uv[4];
  float r0 = step(0.5, nrand(float2(IN.vid, 0)));
  float r1 = step(0.5, nrand(float2(0, IN.vid)));
  float r2 = step(0.5, nrand(float2(IN.vid, IN.vid)));
  uv[0] = lerp(ouv[0], lerp(ouv[1], lerp(ouv[3], ouv[2], r0), r1), r2);
  uv[1] = lerp(ouv[1], lerp(ouv[3], lerp(ouv[2], ouv[0], r0), r1), r2);
  uv[2] = lerp(ouv[2], lerp(ouv[0], lerp(ouv[1], ouv[3], r0), r1), r2);
  uv[3] = lerp(ouv[3], lerp(ouv[2], lerp(ouv[0], ouv[1], r0), r1), r2);

  o = vert_out(IN, o, p[0], wnrm);
  o.uv = uv[0];
#if defined(PASS_CUBE_SHADOWCASTER)
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  o.wuv = wuv[0];
#endif
  OUT.Append(o);

  o = vert_out(IN, o, p[1], wnrm);
  o.uv = uv[1];
#if defined(PASS_CUBE_SHADOWCASTER)
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  o.wuv = wuv[1];
#endif
  OUT.Append(o);

  o = vert_out(IN, o, p[2], wnrm);
  o.uv = uv[2];
#if defined(PASS_CUBE_SHADOWCASTER)
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  o.wuv = wuv[2];
#endif
  OUT.Append(o);

  o = vert_out(IN, o, p[3], wnrm);
  o.uv = uv[3];
#if defined(PASS_CUBE_SHADOWCASTER)
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  o.wuv = wuv[3];
#endif
  OUT.Append(o);

  OUT.RestartStrip();
}

[maxvertexcount(24)]
void geom(point Attributes IN[1], inout TriangleStream<Varyings> OUT)
{
  float3 halfS = 0.5f * IN[0].size;

  float3 pos = IN[0].position.xyz;
  float3 right = rotate_vector(float3(1, 0, 0), IN[0].rotation) * halfS.x;
  float3 up = rotate_vector(float3(0, 1, 0), IN[0].rotation) * halfS.y;
  float3 forward = rotate_vector(float3(0, 0, 1), IN[0].rotation) * halfS.z;

  // wuv for forward faces
  // a forward face is front side of bird view

  float3 origin = IN[0].origin;
  float3 prt = mul(unity_ObjectToWorld, float4(origin + float3( halfS.x, -halfS.y, 0), 1)).xyz;
  float3 prb = mul(unity_ObjectToWorld, float4(origin + float3( halfS.x,  halfS.y, 0), 1)).xyz;
  float3 plt = mul(unity_ObjectToWorld, float4(origin + float3(-halfS.x, -halfS.y, 0), 1)).xyz;
  float3 plb = mul(unity_ObjectToWorld, float4(origin + float3(-halfS.x,  halfS.y, 0), 1)).xyz;

  float2 wuv[4];
  wuv[0] = (prt.xz - _CaptureBoundsMin.xz) / _CaptureBoundsSize.xz + _TileOffset;
  wuv[1] = (prb.xz - _CaptureBoundsMin.xz) / _CaptureBoundsSize.xz + _TileOffset;
  wuv[2] = (plt.xz - _CaptureBoundsMin.xz) / _CaptureBoundsSize.xz + _TileOffset;
  wuv[3] = (plb.xz - _CaptureBoundsMin.xz) / _CaptureBoundsSize.xz + _TileOffset;

  float4 v[4];

	// forward
  v[0] = float4(pos + forward + right - up, 1.0f);
  v[1] = float4(pos + forward + right + up, 1.0f);
  v[2] = float4(pos + forward - right - up, 1.0f);
  v[3] = float4(pos + forward - right + up, 1.0f);
  add_face(IN[0], OUT, v, normalize(forward), wuv);

	// back
  v[0] = float4(pos - forward - right - up, 1.0f);
  v[1] = float4(pos - forward - right + up, 1.0f);
  v[2] = float4(pos - forward + right - up, 1.0f);
  v[3] = float4(pos - forward + right + up, 1.0f);
  add_face(IN[0], OUT, v, -normalize(forward), wuv);

	// up
  v[0] = float4(pos - forward - right + up, 1.0f);
  v[1] = float4(pos + forward - right + up, 1.0f);
  v[2] = float4(pos - forward + right + up, 1.0f);
  v[3] = float4(pos + forward + right + up, 1.0f);
  add_face(IN[0], OUT, v, normalize(up), wuv);

	// down
  v[0] = float4(pos - forward + right - up, 1.0f);
  v[1] = float4(pos + forward + right - up, 1.0f);
  v[2] = float4(pos - forward - right - up, 1.0f);
  v[3] = float4(pos + forward - right - up, 1.0f);
  add_face(IN[0], OUT, v, -normalize(up), wuv);

	// left
  v[0] = float4(pos - forward - right - up, 1.0f);
  v[1] = float4(pos + forward - right - up, 1.0f);
  v[2] = float4(pos - forward - right + up, 1.0f);
  v[3] = float4(pos + forward - right + up, 1.0f);
  add_face(IN[0], OUT, v, -normalize(right), wuv);

	// right
  v[0] = float4(pos - forward + right + up, 1.0f);
  v[1] = float4(pos + forward + right + up, 1.0f);
  v[2] = float4(pos - forward + right - up, 1.0f);
  v[3] = float4(pos + forward + right - up, 1.0f);
  add_face(IN[0], OUT, v, normalize(right), wuv);
};

void edgarize(float2 uv)
{
  float ex = (step(uv.x, _Edge) + step(1.0 - _Edge, uv.x));
  float ey = (step(uv.y, _Edge) + step(1.0 - _Edge, uv.y));
  clip((ex + ey) - 0.1);
}

#if defined(PASS_CUBE_SHADOWCASTER)

half4 frag(Varyings input) : SV_Target
{
  edgarize(input.uv);
  float depth = length(input.shadow) + unity_LightShadowBias.x;
  return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

half4 frag() : SV_Target { return 0; }

#else

void frag(Varyings IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
{
  edgarize(IN.uv);

  half3 albedo = IN.color.rgb * _Color.rgb;

  half3 pack = tex2D(_Pack, IN.uv2 + (IN.uv - (0.5).xx) * _PackSize).rgb;
  albedo = lerp(albedo, pack, _UsePack);

  // float2 wuv = (IN.wpos.xz - _CaptureBoundsMin.xz) / _CaptureBoundsSize.xz;
  half3 capture = tex2D(_CaptureTex, IN.wuv).rgb;
  albedo = lerp(albedo, capture, _UseCapture);

  half3 c_diff, c_spec;
  half refl10;
  c_diff = DiffuseAndSpecularFromMetallic(
    albedo, _Metallic, // input
    c_spec, refl10 // out
  );

  UnityStandardData data;
  data.diffuseColor = c_diff;
  data.occlusion = 1.0;
  data.specularColor = c_spec;
  data.smoothness = _Glossiness;
  data.normalWorld = IN.normal;
  UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

  half3 sh = ShadeSHPerPixel(data.normalWorld, IN.ambient, IN.wpos);
  float4 emission = IN.emission * float4(albedo, 1);
  outEmission = emission + half4(sh * c_diff, 1);
}

#endif
