using UnityEngine;

namespace Pathfinding
{
    public class AStarEdge
    {
        public AStarNode a => _a;
        public AStarNode b => _b;

        readonly AStarNode _a;
        readonly AStarNode _b;

        public AStarEdge(AStarNode a, AStarNode b)
        {
            _a = a;
            _b = b;
        }

        public override bool Equals(object obj)
        {
            return obj is AStarEdge other && ((other.a == a && other.b == b) || (other.a == b && other.b == a));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }
    }

    public static class EdgeUtils
    {
        public static float GetMaxDistance(this AStarEdge edge, Vector3 from)
        {
            float aDistance = Vector3.Distance(edge.a.octreeNode.bounds.center, from);
            float bDistance = Vector3.Distance(edge.b.octreeNode.bounds.center, from);

            return Mathf.Max(aDistance, bDistance);
        }

        public static float GetMaxDistance(this FlowFieldGraph.Edge edge, Vector3 from)
        {
            float aDistance = Vector3.Distance(edge.a.octreeNode.bounds.center, from);
            float bDistance = Vector3.Distance(edge.b.octreeNode.bounds.center, from);

            return Mathf.Max(aDistance, bDistance);
        }
    }
}
