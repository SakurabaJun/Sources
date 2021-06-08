using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Refleconnect
{
    /// <summary>
    /// ゲームで使われる計算
    /// </summary>
    static class Calculation
    {
        //Sphereの当たり判定
        static public Vector3 HitSphere(Vector3 opponent,Vector3 self,ref Vector3 vector)
        {
            double sokuV0 = Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z);
            double kaku1 = Math.Atan2(opponent.Z - self.Z, opponent.X - self.X);
            double kaku2 = Math.Atan2(vector.Z, vector.X);
            double kakuSa = kaku2 - kaku1;
            double sokuV1 = Math.Abs(sokuV0 * Math.Cos(kakuSa));
            double sokuV2 = Math.Abs(sokuV0 * Math.Sin(kakuSa));
            double x;
            double z;
            if (Math.Sin(kakuSa) < 0)
            {
                x = sokuV2 * Math.Cos(kaku1 - Math.PI / 2);
                z = sokuV2 * Math.Sin(kaku1 - Math.PI / 2);
            }
            else
            {
                x = sokuV2 * Math.Cos(kaku1 + Math.PI / 2);
                z = sokuV2 * Math.Sin(kaku1 + Math.PI / 2);
            }
            vector.X = (float)x;
            vector.Z = (float)z;
            vector = Vector3.Normalize(vector);
            vector = NanAvoidance(vector);
            x = sokuV1 * Math.Cos(kaku1);
            z = sokuV1 * Math.Sin(kaku1);
            return new Vector3((float)x, 0, (float)z);
        }

        //Nanが出た場合は0にする
        static public Vector3 NanAvoidance(Vector3 value)
        {
            if (float.IsNaN(value.X))
            {
                value.X = 0;
            }
            if (float.IsNaN(value.Y))
            {
                value.Y = 0;
            }
            if (float.IsNaN(value.Z))
            {
                value.Z = 0;
            }
            return value;
        }

        //ラジアンの取得
        static public double GetRadian(double x, double y, double x2, double y2)
        {
            return Math.Atan2(y2 - y, x2 - x);
        }

        //Clamp
        static public T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        //0より小さい場合の角度に360を足して補正する
        static public float Degree360Conversion(float value)
        {
            if (value < 0)
            {
                return 360 + value;
            }
            return value;
        }

        //角度の最短を探す
        static public void RotationShortes(ref float value1, ref float value2)
        {
            float difference = value1 - value2;
            if (difference < -180)
            {
                value1 += 360;
                return;
            }
            if (difference > 180)
            {
                value1 -= 360;
            }
        }

        //FInterp(UE4にあったため自作)
        static public float Finterp(float value1, float value2, float amount)
        {
            if ((value1 - value2) > 0)
            {
                if (value1 - amount <= value2)
                {
                    return value2;
                }
                return value1 - amount;
            }
            else if (value1 - value2 < 0)
            {
                if (value1 + amount >= value2)
                {
                    return value2;
                }
                return value1 + amount;
            }
            else
            {
                return value1;
            }
        }

        //距離を取得
        static public double GetDistance(double x, double y, double x2, double y2)
        {
            double distance = Math.Sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y));
            return distance;
        }
    }
}
