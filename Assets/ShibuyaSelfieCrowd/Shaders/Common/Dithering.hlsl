#ifndef __DITHERING_COMMON_INCLUDED__
#define __DITHERING_COMMON_INCLUDED__

#include "UnityCG.cginc"

float dithered(float2 pos, float alpha)
{
  pos *= _ScreenParams.xy;

  static const float DITHER_THRESHOLDS[16] =
  {
    1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
    4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
  };

  uint index = (uint(pos.x) % 4) * 4 + (uint(pos.y) % 4);
  return alpha - DITHER_THRESHOLDS[index];
}

void dither_clip(float2 pos, float alpha)
{
  clip(dithered(pos, alpha));
}

#endif
