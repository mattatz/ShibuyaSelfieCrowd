
#include "Assets/Common/Shaders/Random.hlsl"
#include "Assets/Common/Shaders/Easing.hlsl"

float _EmissionRate, _EmissionOffset;
float _EmissionRandom;

float4 emission(int iid, float4 emissionBase, float4 emissionHighlight)
{
  float x = nrand(float2(13.7, iid));
  float t = _EmissionOffset + x * _EmissionRandom;

  float it = floor(t);
  float ft = t - it;
  float n = nrand(float2(iid, it));

  return lerp(emissionBase, emissionHighlight, ease_out_quad(1 - ft) * step(saturate(n), _EmissionRate - 1e-8));
}
