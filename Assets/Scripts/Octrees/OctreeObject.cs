using UnityEngine;

namespace Octrees
{
    public class OctreeObject
    {
        readonly Bounds _bounds;

        public OctreeObject(Vector3 collisionPosition)
        {
            _bounds = new Bounds(collisionPosition, Vector3.one);
        }

        public bool Intersects(Bounds boundsToCheck)
        {
            return _bounds.Intersects(boundsToCheck);
        }
    }
}
