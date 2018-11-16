#ifndef __VPARTICLE_COMMON_INCLUDED__
#define __VPARTICLE_COMMON_INCLUDED__

struct VParticle
{
  float3 position;
  float3 origin;
  float4 rotation;
  float3 scale;
  float2 uv;
  float3 velocity;
  float speed;
  float lifetime;
  bool flow;
};

#endif
