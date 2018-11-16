#ifndef __HUMAN_COMMON_INCLUDED__
#define __HUMAN_COMMON_INCLUDED__

struct Human
{
  float3 position;
  float4 rotation;
  float3 scale;
  float3 velocity;
  float lifetime;
  bool alive;
};

#endif
