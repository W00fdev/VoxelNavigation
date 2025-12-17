#if !UNITY_DOTSPLAYER
using UnityEngine;

#pragma warning disable 0660, 0661

namespace Fraktalia.Core.Mathematics
{
    public partial struct Ffloat2
    {
        /// <summary>
        /// Converts a float2 to Vector2.
        /// </summary>
        /// <param name="v">float2 to convert.</param>
        /// <returns>The converted Vector2.</returns>
        public static implicit operator Vector2(Ffloat2 v)     { return new Vector2(v.x, v.y); }

        /// <summary>
        /// Converts a Vector2 to float2.
        /// </summary>
        /// <param name="v">Vector2 to convert.</param>
        /// <returns>The converted float2.</returns>
        public static implicit operator Ffloat2(Vector2 v)     { return new Ffloat2(v.x, v.y); }
    }

    public partial struct Ffloat3
    {
        /// <summary>
        /// Converts a float3 to Vector3.
        /// </summary>
        /// <param name="v">float3 to convert.</param>
        /// <returns>The converted Vector3.</returns>
        public static implicit operator Vector3(Ffloat3 v)     { return new Vector3(v.x, v.y, v.z); }

        /// <summary>
        /// Converts a Vector3 to float3.
        /// </summary>
        /// <param name="v">Vector3 to convert.</param>
        /// <returns>The converted float3.</returns>
        public static implicit operator Ffloat3(Vector3 v)     { return new Ffloat3(v.x, v.y, v.z); }
    }

    public partial struct Ffloat4
    {
        /// <summary>
        /// Converts a Vector4 to float4.
        /// </summary>
        /// <param name="v">Vector4 to convert.</param>
        /// <returns>The converted float4.</returns>
        public static implicit operator Ffloat4(Vector4 v)     { return new Ffloat4(v.x, v.y, v.z, v.w); }

        /// <summary>
        /// Converts a float4 to Vector4.
        /// </summary>
        /// <param name="v">float4 to convert.</param>
        /// <returns>The converted Vector4.</returns>
        public static implicit operator Vector4(Ffloat4 v)     { return new Vector4(v.x, v.y, v.z, v.w); }
    }

    public partial struct Fquaternion
    {
        /// <summary>
        /// Converts a quaternion to Quaternion.
        /// </summary>
        /// <param name="q">quaternion to convert.</param>
        /// <returns>The converted Quaternion.</returns>
        public static implicit operator Quaternion(Fquaternion q)  { return new Quaternion(q.value.x, q.value.y, q.value.z, q.value.w); }

        /// <summary>
        /// Converts a Quaternion to quaternion.
        /// </summary>
        /// <param name="q">Quaternion to convert.</param>
        /// <returns>The converted quaternion.</returns>
        public static implicit operator Fquaternion(Quaternion q)  { return new Fquaternion(q.x, q.y, q.z, q.w); }
    }

    public partial struct Ffloat4x4
    {
        /// <summary>
        /// Converts a Matrix4x4 to float4x4.
        /// </summary>
        /// <param name="m">Matrix4x4 to convert.</param>
        /// <returns>The converted float4x4.</returns>
        public static implicit operator Ffloat4x4(Matrix4x4 m) { return new Ffloat4x4(m.GetColumn(0), m.GetColumn(1), m.GetColumn(2), m.GetColumn(3)); }

        /// <summary>
        /// Converts a float4x4 to Matrix4x4.
        /// </summary>
        /// <param name="m">float4x4 to convert.</param>
        /// <returns>The converted Matrix4x4.</returns>
        public static implicit operator Matrix4x4(Ffloat4x4 m) { return new Matrix4x4(m.c0, m.c1, m.c2, m.c3); }
    }
}
#endif
