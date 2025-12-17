using UnityEditor;

#if UNITY_EDITOR
namespace Fraktalia.Core.Mathematics.Editor
{
    [CustomPropertyDrawer(typeof(Fquaternion))]
    class FQuaternionDrawer : FPostNormalizedVectorDrawer
    {
        protected override SerializedProperty GetVectorProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("value");
        }

        protected override Fdouble4 Normalize(Fdouble4 value)
        {
            return Fmath.normalizesafe(new Fquaternion((Ffloat4)value)).value;
        }
    }
}
#endif