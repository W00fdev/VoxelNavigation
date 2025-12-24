using UnityEngine;

namespace Pathfinding
{
    public class Edge
    {
        public Node a => _a;
        public Node b => _b;

        readonly Node _a;
        readonly Node _b;

        public Edge(Node a, Node b)
        {
            _a = a;
            _b = b;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && ((other.a == a && other.b == b) || (other.a == b && other.b == a));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }
    }

    public static class EdgeUtils
    {
        public static float GetMaxDistance(this Edge edge, Vector3 from)
        {
            float aDistance = Vector3.Distance(edge.a.octreeNode.bounds.center, from);
            float bDistance = Vector3.Distance(edge.b.octreeNode.bounds.center, from);

            return Mathf.Max(aDistance, bDistance);
        }
    }
}
