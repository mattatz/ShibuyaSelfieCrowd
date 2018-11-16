
#include "Assets/Common/Shaders/Random.hlsl"
#include "Assets/Common/Shaders/Easing.hlsl"

float _EmissionRate, _EmissionSpeed;
float _EmissionOffset, _EmissionRandom;

float4 emission(int id, float4 emissionBase, float4 emissionHighlight)
{
  float x = nrand(float2(13.7, id));
  float t = _EmissionOffset + x * _EmissionRandom;
  float it = floor(t);
  float ft = t - it;
  float y = nrand(float2(id, it));

  return lerp(emissionBase, emissionHighlight, ease_out_quad(1 - ft) * step(saturate(y), _EmissionRate - 1e-8));
}
