namespace Fraktalia.Core.Mathematics
{
    /// <summary>
    /// Used by property drawers when vectors should be post normalized.
    /// </summary>
    public class FPostNormalizeAttribute : UnityEngine.PropertyAttribute {}

    /// <summary>
    /// Used by property drawers when vectors should not be normalized.
    /// </summary>
    public class FDoNotNormalizeAttribute : UnityEngine.PropertyAttribute {}
}
