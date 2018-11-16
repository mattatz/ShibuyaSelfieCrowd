using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VJ {

    public class OSCUtils {

        public static int GetIValue(List<object> data, int index = 0, int def = 0)
        {
            if(data.Count <= index) {
                return def;
            }

            int result;
            if(int.TryParse(data[index].ToString(), out result)) {
                return result;
            }
            return def;
        }

        public static float GetFValue(List<object> data, int index = 0, float def = 0f)
        {
            if(data.Count <= index) {
                return def;
            }

            float result;
            if(float.TryParse(data[index].ToString(), out result)) {
                return result;
            }
            return def;
        }

        public static bool GetBoolFlag(List<object> data, int index = 0, bool def = false)
        {
            if(data.Count <= index) {
                return def;
            }

            int flag;
            if(int.TryParse(data[index].ToString(), out flag)) {
                return flag == 1;
            }
            return def;
        }

        public static Vector2 GetV2Value(List<object> data)
        {
            var v = new Vector2();

            float x, y;
            if(
                float.TryParse(data[0].ToString(), out x) &&
                float.TryParse(data[1].ToString(), out y)
            ) {
                v.Set(x, y);
            }

            return v;
        }

        public static Vector3 GetV3Value(List<object> data)
        {
            var v = new Vector3();

            float x, y, z;
            if(
                float.TryParse(data[0].ToString(), out x) &&
                float.TryParse(data[1].ToString(), out y) &&
                float.TryParse(data[2].ToString(), out z)
            ) {
                v.Set(x, y, z);
            }

            return v;
        }

        public static Color GetColorValue(List<object> data)
        {
            Color color = Color.black;
            const float inv = 1f / 255f;

            float r, g, b;
            if(
                float.TryParse(data[0].ToString(), out r) &&
                float.TryParse(data[1].ToString(), out g) &&
                float.TryParse(data[2].ToString(), out b)
            ) {
                color = new Color(r * inv, g * inv, b * inv);
            }

            return color;
        }

        public static Color GetHDRColorValue(List<object> data)
        {
            Color color = Color.black;
            const float inv = 1f / 255f;

            float r, g, b, brightness;
            if(
                float.TryParse(data[0].ToString(), out r) &&
                float.TryParse(data[1].ToString(), out g) &&
                float.TryParse(data[2].ToString(), out b) && 
                float.TryParse(data[3].ToString(), out brightness)
            ) {
                color = new Color(r * inv * brightness, g * inv * brightness, b * inv * brightness);
            }

            return color;
        }



    }

}


