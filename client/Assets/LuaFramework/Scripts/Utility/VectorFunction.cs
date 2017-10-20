using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace LuaFramework
{
    public class VectorFunction
    {
        public static float Distance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        public static Vector3 MoveTowards(Vector3 cur, Vector3 to, float dis)
        {
            return Vector3.MoveTowards(cur, to, dis);
        }

        public static Vector2 MoveTowards(Vector2 cur, Vector2 to, float dis)
        {
            return Vector2.MoveTowards(cur, to, dis);
        }

        public static Vector2 GetRandomVector2(float len)
        {
            float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            return new Vector2(len * Mathf.Cos(angle), len * Mathf.Sin(angle));
        }
    }
}
