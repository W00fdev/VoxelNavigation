using UnityEngine;

namespace Octrees
{
    public class OctreeObject
    {
        private Bounds bounds;

        public OctreeObject(GameObject gameObject)
        {
            bounds = gameObject.GetComponent<Collider>().bounds;
        }

        public bool Intersects(Bounds boundsToCheck)
        {
            return bounds.Intersects(boundsToCheck);
        }
    }
}