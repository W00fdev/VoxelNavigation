using System;
using System.Collections.Generic;
using Octrees;

namespace Pathfinding
{
    public class AStarNode : IEquatable<AStarNode>
    {
        public int id => _id;
        public OctreeNode octreeNode => _octreeNode;
        public List<AStarEdge> edges => _edges;

        public float f;
        public float g;
        public float h;
        public AStarNode from;

        readonly int _id;
        readonly OctreeNode _octreeNode;
        readonly List<AStarEdge> _edges = new();

        static int _nextId;

        public AStarNode(OctreeNode octreeNode)
        {
            _id = _nextId++;
            _octreeNode = octreeNode;
        }

        public bool Equals(AStarNode other)
        {
            return other != null && id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is AStarNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}