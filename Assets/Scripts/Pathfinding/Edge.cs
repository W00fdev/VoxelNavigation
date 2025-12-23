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
}
