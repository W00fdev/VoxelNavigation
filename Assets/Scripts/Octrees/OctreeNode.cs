using System.Collections.Generic;
using UnityEngine;

namespace Octrees
{
    public class OctreeNode
    {
        public bool isLeaf => children == null;
        public int id => _id;
        public Bounds bounds => _bounds;
        public List<OctreeObject> objects => _objects;
        public OctreeNode[] children;

        readonly int _id;
        readonly float _minNodeSize;
        readonly Bounds _bounds;
        readonly Bounds[] _childBounds = new Bounds[8];
        readonly List<OctreeObject> _objects = new();
        OctreeNode[] _children;

        static int _nextId;

        public OctreeNode(Bounds bounds, float minNodeSize)
        {
            _id = _nextId++;

            _bounds = bounds;
            _minNodeSize = minNodeSize;

            Vector3 newSize = bounds.size * 0.5f;
            Vector3 centerOffset = bounds.size * 0.25f;
            Vector3 parentCenter = bounds.center;

            for (int i = 0; i < 8; i++)
            {
                Vector3 childCenter = parentCenter;

                childCenter.x += centerOffset.x
                  * ((i & 1) == 0
                        ? -1
                        : 1);

                childCenter.y += centerOffset.y
                  * ((i & 2) == 0
                        ? -1
                        : 1);

                childCenter.z += centerOffset.z
                  * ((i & 4) == 0
                        ? -1
                        : 1);

                _childBounds[i] = new Bounds(childCenter, newSize);
            }
        }

        public void Divide(Vector3 collisionPosition)
        {
            Divide(new OctreeObject(collisionPosition));
        }

        void Divide(OctreeObject octreeObject)
        {
            if (_bounds.size.x <= _minNodeSize)
            {
                AddObject(octreeObject);
                return;
            }

            _children ??= new OctreeNode[8];

            bool intersectsChild = false;

            for (int i = 0; i < 8; i++)
            {
                _children[i] ??= new OctreeNode(_childBounds[i], _minNodeSize);

                if (octreeObject.Intersects(_childBounds[i]))
                {
                    _children[i]
                       .Divide(octreeObject);

                    intersectsChild = true;
                }
            }

            if (!intersectsChild)
            {
                AddObject(octreeObject);
            }
        }

        void AddObject(OctreeObject octreeObject) => _objects.Add(octreeObject);

        public void DrawNode()
        {
            /*Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size * 1f);*/

            foreach (OctreeObject octreeObject in _objects)
            {
                if (octreeObject.Intersects(_bounds))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(_bounds.center, _bounds.size);
                }
            }

            if (_children != null)
            {
                foreach (OctreeNode childOctreeNode in _children)
                {
                    childOctreeNode.DrawNode();
                }
            }
        }
    }
}
