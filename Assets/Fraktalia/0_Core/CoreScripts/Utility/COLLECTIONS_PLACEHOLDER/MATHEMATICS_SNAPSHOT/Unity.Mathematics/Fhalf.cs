using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Fraktalia.Core.Mathematics
{
    /// <summary>
    /// A half precision float that uses 16 bits instead of 32 bits.
    /// </summary>
    [FIl2CppEagerStaticClassConstruction]
    [Serializable]
    public struct Fhalf : System.IEquatable<Fhalf>, IFormattable
    {
        /// <summary>
        /// The raw 16 bit value of the half.
        /// </summary>
        public ushort value;

        /// <summary>half zero value.</summary>
        public static readonly Fhalf zero = new Fhalf();

        /// <summary>
        /// The maximum finite half value as a single precision float.
        /// </summary>
        public static float MaxValue { get { return 65504.0f; } }

        /// <summary>
        /// The minimum finite half value as a single precision float.
        /// </summary>
        public static float MinValue { get { return -65504.0f; } }

        /// <summary>
        /// The maximum finite half value as a half.
        /// </summary>
        public static Fhalf MaxValueAsHalf => new Fhalf(MaxValue);

        /// <summary>
        /// The minimum finite half value as a half.
        /// </summary>
        public static Fhalf MinValueAsHalf => new Fhalf(MinValue);

        /// <summary>Constructs a half value from a half value.</summary>
        /// <param name="x">The input half value to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fhalf(Fhalf x)
        {
            value = x.value;
        }

        /// <summary>Constructs a half value from a float value.</summary>
        /// <param name="v">The single precision float value to convert to half.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fhalf(float v)
        {
            value = (ushort)Fmath.f32tof16(v);
        }

        /// <summary>Constructs a half value from a double value.</summary>
        /// <param name="v">The double precision float value to convert to half.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fhalf(double v)
        {
            value = (ushort)Fmath.f32tof16((float)v);
        }

        /// <summary>Explicitly converts a float value to a half value.</summary>
        /// <param name="v">The single precision float value to convert to half.</param>
        /// <returns>The converted half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Fhalf(float v) { return new Fhalf(v); }

        /// <summary>Explicitly converts a double value to a half value.</summary>
        /// <param name="v">The double precision float value to convert to half.</param>
        /// <returns>The converted half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Fhalf(double v) { return new Fhalf(v); }

        /// <summary>Implicitly converts a half value to a float value.</summary>
        /// <param name="d">The half value to convert to a single precision float.</param>
        /// <returns>The converted single precision float value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Fhalf d) { return Fmath.f16tof32(d.value); }

        /// <summary>Implicitly converts a half value to a double value.</summary>
        /// <param name="d">The half value to convert to double precision float.</param>
        /// <returns>The converted double precision float value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator double(Fhalf d) { return Fmath.f16tof32(d.value); }


        /// <summary>Returns whether two half values are bitwise equivalent.</summary>
        /// <param name="lhs">Left hand side half value to use in comparison.</param>
        /// <param name="rhs">Right hand side half value to use in comparison.</param>
        /// <returns>True if the two half values are bitwise equivalent, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fhalf lhs, Fhalf rhs) { return lhs.value == rhs.value; }

        /// <summary>Returns whether two half values are not bitwise equivalent.</summary>
        /// <param name="lhs">Left hand side half value to use in comparison.</param>
        /// <param name="rhs">Right hand side half value to use in comparison.</param>
        /// <returns>True if the two half values are not bitwise equivalent, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fhalf lhs, Fhalf rhs) { return lhs.value != rhs.value; }


        /// <summary>Returns true if the half is bitwise equivalent to a given half, false otherwise.</summary>
        /// <param name="rhs">Right hand side half value to use in comparison.</param>
        /// <returns>True if the half value is bitwise equivalent to the input, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Fhalf rhs) { return value == rhs.value; }

        /// <summary>Returns true if the half is equal to a given half, false otherwise.</summary>
        /// <param name="o">Right hand side object to use in comparison.</param>
        /// <returns>True if the object is of type half and is bitwise equivalent, false otherwise.</returns>
        public override bool Equals(object o) { return o is Fhalf converted && Equals(converted); }

        /// <summary>Returns a hash code for the half.</summary>
        /// <returns>The computed hash code of the half.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return (int)value; }

        /// <summary>Returns a string representation of the half.</summary>
        /// <returns>The string representation of the half.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Fmath.f16tof32(value).ToString();
        }

        /// <summary>Returns a string representation of the half using a specified format and culture-specific format information.</summary>
        /// <param name="format">The format string to use during string formatting.</param>
        /// <param name="formatProvider">The format provider to use during string formatting.</param>
        /// <returns>The string representation of the half.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Fmath.f16tof32(value).ToString(format, formatProvider);
        }
    }

    public static partial class Fmath
    {
        /// <summary>Returns a half value constructed from a half values.</summary>
        /// <param name="x">The input half value to copy.</param>
        /// <returns>The constructed half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fhalf half(Fhalf x) { return new Fhalf(x); }

        /// <summary>Returns a half value constructed from a float value.</summary>
        /// <param name="v">The single precision float value to convert to half.</param>
        /// <returns>The constructed half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fhalf half(float v) { return new Fhalf(v); }

        /// <summary>Returns a half value constructed from a double value.</summary>
        /// <param name="v">The double precision float value to convert to half.</param>
        /// <returns>The constructed half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fhalf half(double v) { return new Fhalf(v); }

        /// <summary>Returns a uint hash code of a half value.</summary>
        /// <param name="v">The half value to hash.</param>
        /// <returns>The computed hash code of the half value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint hash(Fhalf v)
        {
            return v.value * 0x745ED837u + 0x816EFB5Du;
        }
    }
}
