using System.Collections.Generic;
using Octrees;

namespace Pathfinding
{
    public class Node
    {
        public int id => _id;
        public OctreeNode octreeNode => _octreeNode;
        public List<Edge> edges => _edges;

        public float f;
        public float g;
        public float h;
        public Node from;

        readonly int _id;
        readonly OctreeNode _octreeNode;
        readonly List<Edge> _edges = new();

        static int _nextId;

        public Node(OctreeNode octreeNode)
        {
            _id = _nextId++;
            _octreeNode = octreeNode;
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && _id == other.id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
