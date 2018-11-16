using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

using VertexAnimater;

namespace VJ
{
    
    [StructLayout (LayoutKind.Sequential)]
    public struct VertexAnimAttribute {
        public Vector3 scale;
        public Vector3 offset;
        public Vector2 end;
        public float fps;
        public Vector2 texel;
        public int index;
    }

    public static class VertexAnimHelper
    {

        public static VertexAnimAttribute GetAttribute(Material m, int index)
        {
            VertexAnimAttribute attr = new VertexAnimAttribute();

            attr.scale = m.GetVector(ShaderConst.SHADER_SCALE);
            attr.offset = m.GetVector(ShaderConst.SHADER_OFFSET);
            attr.end = m.GetVector(ShaderConst.SHADER_ANIM_END);
            attr.fps = m.GetFloat(ShaderConst.SHADER_FPS);

            var tex = m.GetTexture(ShaderConst.SHADER_ANIM_TEX);
            attr.texel = tex.texelSize;
            attr.index = index;

            return attr;
        }

    }

}


